using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Repositories.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly eStoreDBContext _context;
        private IDbContextTransaction _transaction;
        private ICategoryRepository _categoryRepository;
        private IMemberRepository _memberRepository;
        private IOrderRepository _orderRepository;
        private IProductRepository _productRepository;

        public ICategoryRepository CategoryRepository => _categoryRepository ??= new CategoryRepository(_context);
        public IMemberRepository MemberRepository => _memberRepository ??= new MemberRepository(_context);
        public IOrderRepository OrderRepository => _orderRepository ??= new OrderRepository(_context);
        public IProductRepository ProductRepository => _productRepository ??= new ProductRepository(_context);


        public UnitOfWork(eStoreDBContext context) => _context = context;

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }


        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
