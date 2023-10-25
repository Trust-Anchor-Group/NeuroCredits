AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"ObjectId":Optional(Str(PObjectId)),
	"Account":Required(Str(PAccount)),
	"PNr":Required(Str(PPNr)),
	"Country":Required(Str(PCountry) like "^[A-Z]{2}$"),
	"Amount":Required(Num(PAmount) >= 0),
	"Currency":Required(Str(PCurrency) like "^[A-Z]{3}$"),
	"Period":Required(Duration(PPeriod)),
	"PeriodInterest":Required(Num(PPeriodInterest) >= 0),
	"MaxInstallments":Required(1 <= Int(PMaxInstallments) <= 12)

}:=Posted) ??? BadRequest("Invalid content in request.");

if empty(PAccount) then BadRequest("Account cannot be empty.");
if empty(PPNr) then BadRequest("Personal number cannot be empty.");
if PPeriod <=  Duration("PT0S") then BadRequest("Period must be positive.");

if empty(PObjectId) then
(
	Account:=select top 1 * from TAG.Payments.NeuroCredits.Data.AccountConfiguration where Account=PAccount;
	if exists(Account) then Conflict("Account settings already defined for this account.");
	Account:=Create(TAG.Payments.NeuroCredits.Data.AccountConfiguration)
)
else 
(
	Account:=select top 1 * from TAG.Payments.NeuroCredits.Data.AccountConfiguration where ObjectId=PObjectId;
	if !exists(Account) then NotFound("Account is not found.");

	if Account.Account != PAccount then
	(
		Account2:=select top 1 * from TAG.Payments.NeuroCredits.Data.AccountConfiguration where Account=PAccount;
		if exists(Account2) then Conflict("Account settings already defined for this account.");
	)
);

Account.Account := PAccount;
Account.PersonalNumber := PPNr;
Account.Country := PCountry;
Account.MaxCredit := PAmount;
Account.Currency := PCurrency;
Account.Period := PPeriod;
Account.PeriodInterest := PPeriodInterest;
Account.MaxInstallments:= PMaxInstallments;

if empty(PObjectId) then
(
	Account.Created := Now;
	Account.Updated := System.DateTime.MinValue;

	SaveNewObject(Account);
	LogMessage:="Neuro-Credits™ account settings created.";
)
else
(
	Account.Updated := Now;
	UpdateObject(Account);
	LogMessage:="Neuro-Credits™ account settings updated.";
);

LogNotice(LogMessage,
{
	"Object": PAccount,
	"Actor": User.UserName,
	"Account": PAccount,
	"PersonalNumber": PPNr,
	"Country": PCountry,
	"MaxCredit": PAmount,
	"Currency": PCurrency,
	"Period": PPeriod.ToString(),
	"PeriodInterest": PPeriodInterest,
	"MaxInstallments": PMaxInstallments
});

{
	"ObjectId": Account.ObjectId,
	"Account": PAccount,
	"PersonalNumber": PPNr,
	"Country": PCountry,
	"MaxCredit": PAmount,
	"Currency": PCurrency,
	"Period": PPeriod.ToString(),
	"PeriodInterest": PPeriodInterest,
	"MaxInstallments": PMaxInstallments
}
