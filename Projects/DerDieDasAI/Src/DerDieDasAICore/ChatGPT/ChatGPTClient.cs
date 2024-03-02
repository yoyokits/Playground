// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace DerDieDasAICore.ChatGPT
{
    using OpenAI_API;
    using OpenAI_API.Chat;
    using OpenAI_API.Models;

    public class ChatGPTClient
    {
        #region Properties

        public static ChatGPTClient Instance { get; private set; }

        // Replace 'YOUR_API_KEY' with your actual API key
        public string APIKey { get; }

        // Replace 'YOUR_MESSAGE' with the message you want to send to ChatGPT
        public string Message { get; set; }

        public OpenAIAPI OpenAIAPI { get; }

        private Conversation DefaultConverstation { get; }

        #endregion Properties

        #region Constructors

        private ChatGPTClient(string apiKey)
        {
            APIKey = apiKey;
            this.OpenAIAPI = new OpenAIAPI(APIKey);
            this.DefaultConverstation = this.CreateConversation();
        }

        #endregion Constructors

        #region Methods

        public static ChatGPTClient CreateInstance(string apiKey)
        {
            Instance = new ChatGPTClient(apiKey);
            return Instance;
        }

        public Task<string> Ask(string message)
        {
            /// give instruction as System
            this.DefaultConverstation.AppendSystemMessage(message);
            return this.DefaultConverstation.GetResponseFromChatbotAsync();
        }

        public Conversation CreateConversation()
        {
            var chat = OpenAIAPI.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = 0;
            return chat;
        }

        #endregion Methods
    }
}