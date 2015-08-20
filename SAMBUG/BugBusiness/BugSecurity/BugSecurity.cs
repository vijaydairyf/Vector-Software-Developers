﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using BugBusiness.Interface.BugSecurity;
using BugBusiness.Interface.BugSecurity.DTO;
using BugBusiness.Interface.BugSecurity.Exceptions;
using DataAccess.Interface;
using DataAccess.Interface.Domain;

namespace BugBusiness.BugSecurity
{

    public class BugSecurity : IBugSecurity
    {

        private readonly IDbAuthentication _dbAuthentication;

        public BugSecurity(IDbAuthentication dbAuthentication)
        {
            _dbAuthentication = dbAuthentication;
        }

        public LoginResponse Login(LoginRequest loginRequest)
        {
            User user = _dbAuthentication.GetUserByCredentials(loginRequest.Username, loginRequest.Password);

            if (user == null)
                throw new NotRegisteredException();
            
            var loginResponse = new LoginResponse()
            {
                User = user
            };
        
            return loginResponse;
        }

        //TODO: Farmer allowed to have multiple accounts? Implemented now to not allow it 
        //TODO: Check that System.Net.Mail.MailAddress allows reasonable and also enough emails - otherwise do own regex
        //TODO: Eventually do user email confirmation after registering
        public RegisterResponse Register(RegisterRequest registerRequest)
        {
            
            if (!registerRequest.Username.Equals(registerRequest.UsernameConfirmation) ||
                !registerRequest.Password.Equals(registerRequest.PasswordConfirmation))
            {
                throw new InvalidInputException();
            }

            try
            {
                new MailAddress(registerRequest.Username);
            }
            catch (Exception)
            {
                throw new InvalidInputException();
            }

            bool queryResult = _dbAuthentication.InsertNewUser(registerRequest.Username, registerRequest.Password, registerRequest.FarmName);

            if (queryResult == false)
            {
                throw new UserExistsException();
            }

            return new RegisterResponse() {};

        }

        public RecoverAccountResponse RecoverAccount(RecoverAccountRequest recoverAccountRequest)
        {
            throw new NotImplementedException();
        }
    }
}