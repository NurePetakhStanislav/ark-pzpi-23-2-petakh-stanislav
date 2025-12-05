using CAaR.Models;

public interface IHasUser
{
    int UserID { get; set; }
    User User { get; set; }
}