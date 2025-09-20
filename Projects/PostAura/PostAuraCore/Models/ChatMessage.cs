namespace PostAuraCore.Models;

public enum ChatRole { User, Assistant, System }

public class ChatMessage
{
    public ChatRole Role { get; set; }
    public string Text { get; set; } = string.Empty;
}