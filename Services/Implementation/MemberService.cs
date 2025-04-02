using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using FluentValidation;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Dto.Member;
using BusinessObjects.Entities;
using Services.Security;
using Microsoft.Extensions.Logging;

namespace Services.Implementation
{
    public class MemberService : IMemberService
    {
        private readonly IPasswordService _passwordService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<MemberForCreationDto> _creationValidator;
        private readonly IValidator<MemberForUpdateDto> _updateValidator;
        private readonly ILogger<MemberService> _logger;

        private const int DEFAULT_PAGE_SIZE = 10;
        private const int MAX_PAGE_SIZE = 100;
        private const string ERROR_MEMBER_NOT_FOUND = "Member with ID {0} not found";
        private const string ERROR_EMAIL_EXISTS = "Email {0} is already registered";
        private const string ERROR_INVALID_PAGE = "Page number must be greater than 0";
        private const string ERROR_INVALID_PAGE_SIZE = "Page size must be between 1 and {0}";

        public MemberService(
            IPasswordService passwordService, 
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IValidator<MemberForCreationDto> creationValidator, 
            IValidator<MemberForUpdateDto> updateValidator,
            ILogger<MemberService> logger)
        {
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _creationValidator = creationValidator ?? throw new ArgumentNullException(nameof(creationValidator));
            _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PagedApiResponse<MemberDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                _logger.LogInformation("Getting all members with page number: {PageNumber}, page size: {PageSize}", pageNumber, pageSize);

                IEnumerable<Member> members;
                int totalItems;

                if (!pageNumber.HasValue || !pageSize.HasValue)
                {
                    members = await _unitOfWork.MemberRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = members.Count();
                    pageNumber = 1;
                    pageSize = totalItems;
                }
                else
                {
                    if (pageNumber < 1)
                    {
                        _logger.LogWarning("Invalid page number: {PageNumber}", pageNumber);
                        return (PagedApiResponse<MemberDto>)PagedApiResponse<MemberDto>.ErrorResponse(ERROR_INVALID_PAGE,
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", ERROR_INVALID_PAGE));
                    }

                    if (pageSize < 1 || pageSize > MAX_PAGE_SIZE)
                    {
                        _logger.LogWarning("Invalid page size: {PageSize}", pageSize);
                        return (PagedApiResponse<MemberDto>)PagedApiResponse<MemberDto>.ErrorResponse(
                            string.Format(ERROR_INVALID_PAGE_SIZE, MAX_PAGE_SIZE),
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", 
                                string.Format(ERROR_INVALID_PAGE_SIZE, MAX_PAGE_SIZE)));
                    }

                    members = await _unitOfWork.MemberRepository.GetAllAsync(pageNumber.Value, pageSize.Value);
                    var allMembers = await _unitOfWork.MemberRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = allMembers.Count();
                }

                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(members);
                _logger.LogInformation("Successfully retrieved {Count} members", memberDtos.Count());
                return PagedApiResponse<MemberDto>.SuccessPagedResponse(
                    memberDtos,
                    pageNumber.Value,
                    pageSize.Value,
                    totalItems,
                    "Members retrieved successfully"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all members");
                return new PagedApiResponse<MemberDto>
                {
                    Success = false,
                    Message = "Failed to retrieve members",
                    Errors = new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message)
                };
            }
        }

        public async Task<ApiResponse<MemberDto>> CreateAsync(MemberForCreationDto member)
        {
            try
            {
                _logger.LogInformation("Creating new member with email: {Email}", member.Email);

                if (member == null)
                {
                    _logger.LogWarning("Attempted to create member with null data");
                    return ApiResponse<MemberDto>.ErrorResponse("Member cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));
                }

                var validationResult = await _creationValidator.ValidateAsync(member);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for member creation: {Errors}", string.Join(", ", errors));
                    return ApiResponse<MemberDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                // Check for duplicate email
                var existingMember = await _unitOfWork.MemberRepository.GetByEmailAsync(member.Email);
                if (existingMember != null)
                {
                    _logger.LogWarning("Attempted to create member with duplicate email: {Email}", member.Email);
                    return ApiResponse<MemberDto>.ErrorResponse("Email already exists",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Format(ERROR_EMAIL_EXISTS, member.Email)));
                }

                await _unitOfWork.BeginTransactionAsync();

                var memberEntity = _mapper.Map<Member>(member);

                // Increment the ID by 1
                var lastMember = await _unitOfWork.MemberRepository.GetAllAsync(1, int.MaxValue);
                memberEntity.MemberId = lastMember.Max(m => m.MemberId) + 1;

                // Hash the password
                //memberEntity.Password = _passwordService.HashPassword(member.Password);

                _unitOfWork.MemberRepository.Add(memberEntity);
                await _unitOfWork.CommitTransactionAsync();

                var memberDto = _mapper.Map<MemberDto>(memberEntity);
                _logger.LogInformation("Successfully created member with ID: {MemberId}", memberDto.MemberId);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating member");
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<MemberDto>.ErrorResponse("Failed to create member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<MemberDto>> UpdateAsync(int id, MemberForUpdateDto member)
        {
            try
            {
                _logger.LogInformation("Updating member with ID: {MemberId}", id);

                if (member == null)
                {
                    _logger.LogWarning("Attempted to update member with null data");
                    return ApiResponse<MemberDto>.ErrorResponse("Member cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));
                }

                var validationResult = await _updateValidator.ValidateAsync(member);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Validation failed for member update: {Errors}", string.Join(", ", errors));
                    return ApiResponse<MemberDto>.ErrorResponse("Validation failed",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Join(", ", errors)));
                }

                var existingMember = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (existingMember == null)
                {
                    _logger.LogWarning("Attempted to update non-existent member with ID: {MemberId}", id);
                    return ApiResponse<MemberDto>.ErrorResponse(string.Format(ERROR_MEMBER_NOT_FOUND, id),
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", string.Format(ERROR_MEMBER_NOT_FOUND, id)));
                }

                // Check for duplicate email if email is being updated
                if (!string.IsNullOrEmpty(member.Email) && member.Email != existingMember.Email)
                {
                    var memberWithEmail = await _unitOfWork.MemberRepository.GetByEmailAsync(member.Email);
                    if (memberWithEmail != null)
                    {
                        _logger.LogWarning("Attempted to update member with duplicate email: {Email}", member.Email);
                        return ApiResponse<MemberDto>.ErrorResponse("Email already exists",
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", string.Format(ERROR_EMAIL_EXISTS, member.Email)));
                    }
                }

                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(member, existingMember);

                // Hash the password if it's being updated
                // if (!string.IsNullOrEmpty(member.Password))
                // {
                //     existingMember.Password = _passwordService.HashPassword(member.Password);
                // }

                _unitOfWork.MemberRepository.Update(existingMember);
                await _unitOfWork.CommitTransactionAsync();

                var memberDto = _mapper.Map<MemberDto>(existingMember);
                _logger.LogInformation("Successfully updated member with ID: {MemberId}", memberDto.MemberId);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating member with ID: {MemberId}", id);
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<MemberDto>.ErrorResponse("Failed to update member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting member with ID: {MemberId}", id);

                var member = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    _logger.LogWarning("Attempted to delete non-existent member with ID: {MemberId}", id);
                    return ApiResponse<string>.ErrorResponse(string.Format(ERROR_MEMBER_NOT_FOUND, id),
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", string.Format(ERROR_MEMBER_NOT_FOUND, id)));
                }

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.MemberRepository.Delete(member);
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Successfully deleted member with ID: {MemberId}", id);
                return ApiResponse<string>.SuccessResponse(null, "Member deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting member with ID: {MemberId}", id);
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.ErrorResponse("Failed to delete member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<MemberDto>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Getting member with ID: {MemberId}", id);

                var member = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (member == null)
                {
                    _logger.LogWarning("Attempted to get non-existent member with ID: {MemberId}", id);
                    return ApiResponse<MemberDto>.ErrorResponse(string.Format(ERROR_MEMBER_NOT_FOUND, id),
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", string.Format(ERROR_MEMBER_NOT_FOUND, id)));
                }

                var memberDto = _mapper.Map<MemberDto>(member);
                _logger.LogInformation("Successfully retrieved member with ID: {MemberId}", id);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting member with ID: {MemberId}", id);
                return ApiResponse<MemberDto>.ErrorResponse("Failed to retrieve member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }
    }
}