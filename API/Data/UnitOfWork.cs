using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Interfaces;
using AutoMapper;

namespace API.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;
        public readonly IMapper _mapper;
        public UnitOfWork(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _userRepository = new Lazy<IUserRepository>(() =>  new UserRepository(_context, _mapper));
            _messageRepository = new Lazy<IMessageRepository>(() =>  new MessageRepository(_context, _mapper));
            _likesRepository = new Lazy<ILikesRepository>(() =>  new LikesRepository(_context));
            _photoRepository = new Lazy<IPhotoRepository>(() =>  new PhotoRepository(_context, _mapper));
        }

        
        public IUserRepository UserRepository => _userRepository.Value;
        public IMessageRepository MessageRepository => _messageRepository.Value;
        public ILikesRepository LikesRepository => _likesRepository.Value;
        public IPhotoRepository PhotoRepository => _photoRepository.Value;

        public readonly Lazy<IUserRepository> _userRepository;

        public readonly Lazy<IMessageRepository> _messageRepository;
        public readonly Lazy<ILikesRepository> _likesRepository;
        public readonly Lazy<IPhotoRepository> _photoRepository;

        public async Task<bool> Complete()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }
    }
}