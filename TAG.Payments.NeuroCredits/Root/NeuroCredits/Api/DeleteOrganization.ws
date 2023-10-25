AuthenticateSession(Request,"User");
Authorize(User,"Admin.Payments.NeuroCredits");

({
	"ObjectId":Optional(Str(PObjectId))
}:=Posted) ??? BadRequest("Invalid content in request.");

Organization:=select top 1 * from TAG.Payments.NeuroCredits.Data.OrganizationConfiguration where ObjectId=PObjectId;
if !exists(Organization) then NotFound("Organization is not found.");

DeleteObject(Organization);

LogNotice("Neuro-Credits™ organization settings deleted.",
{
	"Object": Organization.OrganizationName,
	"Actor": User.UserName,
	"OrganizationName": Organization.OrganizationName,
	"OrganizationNumber": Organization.OrganizationNumber,
	"OrganizationCountry": Organization.OrganizationCountry,
	"PersonalNumbers": Organization.PersonalNumbers,
	"PersonalCountries": Organization.PersonalCountries,
	"MaxCredit": Organization.MaxCredit,
	"Currency": Organization.Currency,
	"Period": Organization.Period.ToString(),
	"PeriodInterest": Organization.PeriodInterest,
	"MaxInstallments": Organization.MaxInstallments
});

{
	"ObjectId": Organization.ObjectId,
	"OrganizationName": Organization.OrganizationName,
	"OrganizationNumber": Organization.OrganizationNumber,
	"OrganizationCountry": Organization.OrganizationCountry,
	"PersonalNumbers": Organization.PersonalNumbers,
	"PersonalCountries": Organization.PersonalCountries,
	"MaxCredit": Organization.MaxCredit,
	"Currency": Organization.Currency,
	"Period": Organization.Period.ToString(),
	"PeriodInterest": Organization.PeriodInterest,
	"MaxInstallments": Organization.MaxInstallments
}
