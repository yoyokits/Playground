// ========================================== //
// Developer: Yohanes Wahyu Nurcahyo          //
// Website: https://github.com/yoyokits       //
// ========================================== //

namespace LLM.Models
{
    public enum ChatRole
    {
        System,
        User,
        Assistant
    }

    public sealed record ChatMessage(ChatRole Role, string Content);
}