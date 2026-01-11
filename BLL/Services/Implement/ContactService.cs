using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interface;
using Common.DTO;
using DAL.Models;
using DAL.UnitOfWork;
using Stripe;

namespace BLL.Services.Implement
{
    public class ContactService : IContactService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        public ContactService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }
        public async Task<ResponseDTO> CreateContact(CreateContactDTO contactDTO)
        {
            try
            {
                var contact = new Contact
                {
                    ContactId = Guid.NewGuid(),
                    Name = contactDTO.Name,
                    Email = contactDTO.Email,
                    PhoneNumber = contactDTO.PhoneNumber,
                    Subject = contactDTO.Subject,
                    Message = contactDTO.Message,
            
                };

                await _unitOfWork.ContactRepo.AddAsync(contact);
                await _unitOfWork.SaveAsync();

                // johnvnglobal@gmail.com
                await _emailService.SendEmailContactAsync(contactDTO);

                return new ResponseDTO ("Contact created successfully",200,  true);

            }
            catch (Exception ex)
            {
                return new ResponseDTO ($"An error occurred while creating the contact: {ex.Message}", 500, false);

            }
        }

        public async Task<ResponseDTO> GetAllContacts()
        {
            try
            {
                var contacts =  _unitOfWork.ContactRepo.GetAll();

                var contactDTOs = contacts.Select(c => new ContactDTO
                {
                    ContactId = c.ContactId,
                    Name = c.Name,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Subject = c.Subject,
                    Message = c.Message
                }).ToList();

                return new ResponseDTO ("Contacts retrieved successfully", 200, true, contactDTOs);

            }
            catch (Exception ex)
            {
                return new ResponseDTO ($"An error occurred while retrieving contacts: {ex.Message}", 500, false);

            }
        }
    
    }
}
