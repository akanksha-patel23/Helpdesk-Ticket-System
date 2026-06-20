namespace HelpdeskTicket.Application.DTOs.Comment;

// TODO: Implement in step-by-step coding phase
public class CommentDto {
    public int Id { get; set; }
    public string Body { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByRole { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
}
