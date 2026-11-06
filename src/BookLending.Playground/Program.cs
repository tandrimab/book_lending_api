using BookLending.Domain.Items;
using BookLending.Domain.Loans;
using BookLending.Domain.Members;
using BookLending.Domain.Reminders;

int passed = 0, failed = 0;

void Check(string name, bool condition)
{
    if (condition) { passed++; Console.WriteLine($"  [PASS] {name}"); }
    else { failed++; Console.ForegroundColor = ConsoleColor.Red; Console.WriteLine($"  [FAIL] {name}"); Console.ResetColor(); }
}

void Throws<TException>(string name, Action action) where TException : Exception
{
    try { action(); Check($"{name} (expected {typeof(TException).Name}, nothing thrown)", false); }
    catch (TException) { Check(name, true); }
    catch (Exception ex) { Check($"{name} (expected {typeof(TException).Name}, got {ex.GetType().Name})", false); }
}

void Section(string title) => Console.WriteLine($"\n=== {title} ===");

// ---------- Test data ----------
var address = new Address("221B Baker St", "London", "LDN", "NW16XE");
var alice = new PlaygroundMember("Alice", address, DateTime.Now.AddYears(-1));
var bob = new PlaygroundMember("Bob", address with { Street = "10 Downing St" }, DateTime.Now.AddMonths(-2));

var cleanCode = new Book("Clean Code", "Robert C. Martin", 2008, "Prentice Hall", new IsbnNumber("978-0132350884"));
var ddd = new Book("Domain-Driven Design", "Eric Evans", 2003, "Addison-Wesley", new IsbnNumber("978-0321125217"));
var natGeo = new PlaygroundMagazine("National Geographic", "Various", 2024);

Section("Library items");
Check("Book loan period is 14 days", cleanCode.LoanPeriod() == 14);
Check("Magazine loan period is 7 days", natGeo.LoanPeriod() == 7);
Check("Book stores ISBN", cleanCode.Isbn.Value == "978-0132350884");
Check("Book stores publisher", cleanCode.Publisher == "Prentice Hall");
Check("ISBN record struct has value equality", new IsbnNumber("x") == new IsbnNumber("x"));
Check("Book is a LibraryItem (polymorphism)", ((LibraryItem)cleanCode).LoanPeriod() == 14);

Section("Members");
Check("Member stores name", alice.Name == "Alice");
Check("Address record has value equality", alice.Address == new Address("221B Baker St", "London", "LDN", "NW16XE"));
Check("'with' expression creates a modified copy", bob.Address.Street == "10 Downing St" && bob.Address.City == "London");
Check("Member has an Id (currently never assigned in Member)", alice.Id is not null);

Section("Loan entity");
var freshLoan = new Loan("m1", DateTime.Now, null, 14);
Check("New loan is active", freshLoan.IsActive());
Check("New loan is not overdue", !freshLoan.IsOverdue());
Check("DueDate = LoanDate + loanDays", freshLoan.DueDate == freshLoan.LoanDate.AddDays(14));
Check("DaysOverDue is 0 for a fresh loan", freshLoan.DaysOverDue() == 0);

var oldLoan = new Loan("m1", DateTime.Now.AddDays(-20), null, 14);
Check("Loan past due date is overdue", oldLoan.IsOverdue());
Check("DaysOverDue = 6 for loan taken 20 days ago with 14-day period", oldLoan.DaysOverDue() == 6);

oldLoan.MarkAsReturned(DateTime.Now);
Check("Returned loan is not active", !oldLoan.IsActive());
Check("Returned loan is not overdue even if past due", !oldLoan.IsOverdue());
Check("Returned loan has 0 DaysOverDue", oldLoan.DaysOverDue() == 0);

var a = new Loan("m1", DateTime.Now, null, 1);
var b = new Loan("m1", DateTime.Now, null, 1);
Check("Loan Ids are unique", a.Id != b.Id);

var boundary = new Loan("m1", DateTime.Now.AddDays(-14).AddMinutes(1), null, 14);
Check("Loan due in 1 minute is not yet overdue (boundary)", !boundary.IsOverdue());

Section("LoanService.LendBook");
var service = new LoanService();
var loan1 = service.LendBook(alice, cleanCode);
Check("LendBook returns an active loan", loan1.IsActive());
Check("LendBook records the loan", service.LoanItems.Count == 1);
Check("Loan uses the book's 14-day period", (loan1.DueDate - loan1.LoanDate).Days == 14);
Check("Loan is linked to the member", loan1.MemberId == alice.Id);
Throws<ArgumentNullException>("LendBook(null member) throws", () => service.LendBook(null!, cleanCode));
Throws<ArgumentNullException>("LendBook(null book) throws", () => service.LendBook(alice, null!));
Check("Failed LendBook calls don't add loans", service.LoanItems.Count == 1);

var loan2 = service.LendBook(bob, ddd);
Check("Multiple loans tracked", service.LoanItems.Count == 2);

Section("LoanService.ReturnBook");
service.ReturnBook(loan2);
Check("Returned loan is inactive", !loan2.IsActive());
Check("ReturnedOn is set", loan2.ReturnedOn is not null);
Throws<InvalidOperationException>("Returning an already-returned loan throws", () => service.ReturnBook(loan2));
Throws<ArgumentNullException>("ReturnBook(null) throws", () => service.ReturnBook(null!));

Section("LoanService overdue & stats");
// LendBook always uses DateTime.Now, so add back-dated loans directly to simulate overdue ones.
var overdue1 = new Loan(alice.Id, DateTime.Now.AddDays(-30), null, cleanCode.LoanPeriod()); // 16 days overdue
var overdue2 = new Loan(bob.Id, DateTime.Now.AddDays(-10), null, natGeo.LoanPeriod());      // 3 days overdue
var overdueButReturned = new Loan(bob.Id, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-1), 14);
service.LoanItems.AddRange([overdue1, overdue2, overdueButReturned]);

var overdue = service.GetOverdueLoans(DateTime.Now).ToList();
Check("GetOverdueLoans returns exactly 2 loans", overdue.Count == 2);
Check("Overdue list contains overdue1 and overdue2", overdue.Contains(overdue1) && overdue.Contains(overdue2));
Check("Overdue list excludes returned loans", !overdue.Contains(overdueButReturned));
Check("Overdue list excludes on-time loans", !overdue.Contains(loan1));
Check("Magazine loan 10 days old is 3 days overdue", overdue2.DaysOverDue() == 3);

var futureOverdue = service.GetOverdueLoans(DateTime.Now.AddDays(30)).ToList();
Check("GetOverdueLoans honours currentDate (loan1 overdue 30 days from now)", futureOverdue.Contains(loan1));

var (total, overDueCount) = service.GetLoanStats();
Check("Stats: total = 5", total == 5);
Check("Stats: overdue = 2", overDueCount == 2);

var empty = new LoanService().GetLoanStats();
Check("Stats on empty service = (0, 0)", empty == (0, 0));

Section("Reminders");
IReminderSender[] senders = [new EmailReminder(), new SmsReminder()];
var sw = System.Diagnostics.Stopwatch.StartNew();
await Task.WhenAll(
    from l in overdue
    let member = l.MemberId == alice.Id ? alice : bob
    from sender in senders
    select sender.SendReminderAsync(member, $"Loan {l.Id} is {l.DaysOverDue()} day(s) overdue."));
sw.Stop();


class PlaygroundMember(string name, Address address, DateTime start) : Member(name, address, start);
class PlaygroundMagazine(string title, string author, int year) : Magazine(title, author, year);
