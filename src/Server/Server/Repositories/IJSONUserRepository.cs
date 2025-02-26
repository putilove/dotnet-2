﻿using Server.Model;
using System.Collections.Generic;

namespace Server.Repositories
{
    public interface IJSONUserRepository
    {
        public User GetUser(int id);

        public IEnumerable<User> GetUsers();

        public void AddUser(User element);

        public void UpdateUser(int id, User element);

        public void DeleteUser(int id);

        public void DeleteAllUsers();

        public void SaveData();

        public void LoadData();
    }
}
