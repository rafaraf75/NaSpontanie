﻿namespace NaSpontanie.API.Models
{
    // Tabela pośrednia reprezentująca znajomości między użytkownikami
    public class Friendship
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int FriendId { get; set; }
        public User? Friend { get; set; }
    }
}
