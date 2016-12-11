namespace HyperMapper.Mapper.ActionResults
{
    public class UserError  
    {
        public string MessageForUser { get; private set; }
        public UserError(string messageForUser)
        {
            this.MessageForUser = messageForUser;
        }
    }
}