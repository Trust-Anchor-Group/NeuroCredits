AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"Account":Required(Str(PAccount))
}:=Posted) ??? BadRequest("Invalid content in request.");

Account:=select top 1 * from TAG.Payments.NeuroCredits.Data.AccountConfiguration where Account=PAccount;
if !exists(Account) then NotFound("Account not found.");

TAG.Payments.NeuroCredits.ServiceConfiguration.CurrentPersonalDebt(Account.Account,Account.PersonalNumber,Account.Country);