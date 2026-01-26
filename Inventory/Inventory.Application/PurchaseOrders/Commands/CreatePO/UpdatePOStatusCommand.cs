using MediatR;

public class UpdatePOStatusCommand : IRequest<bool>
{
    public int Id { get; set; }
    public string Status { get; set; }

    public UpdatePOStatusCommand(int id, string status)
    {
        Id = id;
        Status = status;
    }
}