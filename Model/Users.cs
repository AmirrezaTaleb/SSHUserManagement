 namespace SSHUserManagement.Model
{
     public class Users
    {
         public long Id { get; set; }
         public string TelegramId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public long ChatId { get; set; }
        public string ExpDate { get; set; }
        public bool IsDeleted { get; set; }
     }
}
