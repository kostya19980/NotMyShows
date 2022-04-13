using System;
using System.Collections.Generic;

namespace NotMyShows.Services
{
    public class UserManagerResponse
    {
        public string UserId { get; set; }
        public int UserProfileId { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; } 
        public IEnumerable<string> Errors { get; set; }
        public DateTime? ExpireDate { get; set; }
    }
}
