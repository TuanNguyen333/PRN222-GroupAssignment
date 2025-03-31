using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.Base;
using Repositories.Interface;
using Services.Interface;
using BusinessObjects.Dto.Member;
using BusinessObjects.Entities;

namespace Services.Implementation
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private const int DEFAULT_PAGE_SIZE = 10;

        public MemberService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PagedApiResponse<MemberDto>> GetAllAsync(int? pageNumber = null, int? pageSize = null)
        {
            try
            {
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
                        return (PagedApiResponse<MemberDto>)PagedApiResponse<MemberDto>.ErrorResponse("Page number must be greater than 0",
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page number"));

                    if (pageSize < 1)
                        return (PagedApiResponse<MemberDto>)PagedApiResponse<MemberDto>.ErrorResponse("Page size must be greater than 0",
                            new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Invalid page size"));

                    members = await _unitOfWork.MemberRepository.GetAllAsync(pageNumber.Value, pageSize.Value);
                    var allMembers = await _unitOfWork.MemberRepository.GetAllAsync(1, int.MaxValue);
                    totalItems = allMembers.Count();
                }

                var memberDtos = _mapper.Map<IEnumerable<MemberDto>>(members);
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
                if (member == null)
                    return ApiResponse<MemberDto>.ErrorResponse("Member cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                await _unitOfWork.BeginTransactionAsync();

                var memberEntity = _mapper.Map<Member>(member);

                // Increment the ID by 1
                var lastMember = await _unitOfWork.MemberRepository.GetAllAsync(1, int.MaxValue);
                memberEntity.MemberId = lastMember.Max(m => m.MemberId) + 1;

                _unitOfWork.MemberRepository.Add(memberEntity);
                await _unitOfWork.CommitTransactionAsync();

                var memberDto = _mapper.Map<MemberDto>(memberEntity);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member created successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<MemberDto>.ErrorResponse("Failed to create member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<MemberDto>> UpdateAsync(int id, MemberForUpdateDto member)
        {
            try
            {
                if (member == null)
                    return ApiResponse<MemberDto>.ErrorResponse("Member cannot be null",
                        new BusinessObjects.Base.ErrorResponse("VALIDATION_ERROR", "Input data is null"));

                var existingMember = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (existingMember == null)
                    return ApiResponse<MemberDto>.ErrorResponse($"Member with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Member does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _mapper.Map(member, existingMember);
                _unitOfWork.MemberRepository.Update(existingMember);

                await _unitOfWork.CommitTransactionAsync();

                var memberDto = _mapper.Map<MemberDto>(existingMember);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto, "Member updated successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<MemberDto>.ErrorResponse("Failed to update member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            try
            {
                var member = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (member == null)
                    return ApiResponse<string>.ErrorResponse($"Member with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Member does not exist"));

                await _unitOfWork.BeginTransactionAsync();

                _unitOfWork.MemberRepository.Delete(member);
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<string>.SuccessResponse(null, "Member deleted successfully");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<string>.ErrorResponse("Failed to delete member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }

        public async Task<ApiResponse<MemberDto>> GetByIdAsync(int id)
        {
            try
            {
                var member = await _unitOfWork.MemberRepository.GetByIdAsync(id);
                if (member == null)
                    return ApiResponse<MemberDto>.ErrorResponse($"Member with ID {id} not found",
                        new BusinessObjects.Base.ErrorResponse("NOT_FOUND", "Member does not exist"));

                var memberDto = _mapper.Map<MemberDto>(member);
                return ApiResponse<MemberDto>.SuccessResponse(memberDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<MemberDto>.ErrorResponse("Failed to retrieve member",
                    new BusinessObjects.Base.ErrorResponse("INTERNAL_SERVER_ERROR", ex.Message));
            }
        }
    }
}