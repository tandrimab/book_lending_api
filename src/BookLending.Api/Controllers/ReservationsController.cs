using BookLending.Application.Reservations;
using BookLending.Domain.Items;
using BookLending.Domain.Members;
using BookLending.Domain.Reservations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookLending.Api.Controllers;

public record ReserveBookRequest(string MemberId, string Isbn);

public record ReservationResponse(
    string Id,
    string BookId,
    string MemberId,
    DateTime CreatedAt,
    string Status,
    DateTime? HoldExpiresAt)
{
    public static ReservationResponse From(Reservation r) => new(
        r.Id, r.BookId, r.MemberId, r.CreatedAt, r.Status.ToString(), r.HoldExpiresAt);
}

[ApiController]
[Authorize(Roles = "Librarian")]
[Route("reservations")]
public class ReservationsController : ControllerBase
{
    private readonly ReservationService _reservationService;
    private readonly IMemberRepository _members;
    private readonly IBookRepository _books;

    public ReservationsController(ReservationService reservationService, IMemberRepository members, IBookRepository books)
    {
        _reservationService = reservationService;
        _members = members;
        _books = books;
    }

    // Put a member on the waitlist for a book.
    [HttpPost]
    public ActionResult<ReservationResponse> Reserve(ReserveBookRequest request)
    {
        var member = _members.GetMemberById(request.MemberId);
        if (member is null) return NotFound($"Member '{request.MemberId}' not found.");

        var book = _books.GetBookBYIsbn(request.Isbn);
        if (book is null) return NotFound($"Book with ISBN '{request.Isbn}' not found.");

        try
        {
            var reservation = _reservationService.ReserveBook(member, book);
            return Created($"/reservations/{reservation.Id}", ReservationResponse.From(reservation));
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
