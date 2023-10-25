AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"ObjectId":Optional(Str(PObjectId))
}:=Posted) ??? BadRequest("Invalid content in request.");

Account:=select top 1 * from TAG.Payments.NeuroCredits.Data.AccountConfiguration where ObjectId=PObjectId;
if !exists(Account) then NotFound("Account is not found.");

DeleteObject(Account);

LogNotice("Neuro-Credits™ account settings deleted.",
{
	"Object": Account.Account,
	"Actor": User.UserName,
	"Account": Account.Account,
	"PersonalNumber": Account.PersonalNumber,
	"Country": Account.Country,
	"MaxCredit": Account.MaxCredit,
	"Currency": Account.Currency,
	"Period": Account.Period.ToString(),
	"PeriodInterest": Account.PeriodInterest,
	"MaxInstallments": Account.MaxInstallments
});

{
	"ObjectId": Account.ObjectId,
	"Account": Account.Account,
	"PersonalNumber": Account.PersonalNumber,
	"Country": Account.Country,
	"MaxCredit": Account.MaxCredit,
	"Currency": Account.Currency,
	"Period": Account.Period.ToString(),
	"PeriodInterest": Account.PeriodInterest,
	"MaxInstallments": Account.MaxInstallments
}
