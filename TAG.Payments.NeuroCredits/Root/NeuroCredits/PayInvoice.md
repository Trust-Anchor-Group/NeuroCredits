Title: Invoice {{Nr}}
Description: Allows a user to pay the invoice {{Nr}}.
Date: 2023-11-07
Author: Peter Waher
Javascript: Settings.js
Javascript: /Events.js
Javascript: /Checkout.js
CSS: PayInvoice.cssx
Cache-Control: max-age=0, no-cache, no-store
Parameter: Nr
Parameter: Key

========================================================================

Invoice {{
if empty(Key) or empty(Nr) then Forbidden("Access denied.");
Invoice:=select top 1 * from TAG.Payments.NeuroCredits.Data.Invoice where InvoiceNumber=Nr;
if !exists(Invoice) then Forbidden("Access denied.");
if Invoice.Key!=Key then Forbidden("Access denied.");
OneRow(s):=empty(s)?"":s.Replace("\r\n","\n").Replace("\r","\n").Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("\n","<br/>");
DurationToString(D):=
(
	M:=[[D.Years,D.Months,D.Days,D.Hours,D.Minutes,D.Seconds],[
		TextUnit(D.Years,"year","years"),
		TextUnit(D.Months,"month","months"),
		TextUnit(D.Days,"day","days"),
		TextUnit(D.Hours,"hour","hours"),
		TextUnit(D.Minutes,"minute","minutes"),
		TextUnit(D.Seconds,"second","seconds")]]T;
	M:=[x in M: x[0]!=0];
	v:=[foreach x in M do x[0]+" "+x[1]];
	s:=concat(v,", ");
	k:=s.LastIndexOf(", ");
	if k>=0 then s:=s.Remove(k,2).Insert(k," and ");
	s
);
Nr}}
=================

<div id="SimpleInformation" style="display:block">

<p>
<span class="simpleName">
{{
if Invoice.IsOrganizational then 
	]]((Invoice.OrganizationName))[[
else
	]]((Invoice.PersonalName))[[;
}}
</span>
<br/>
<span class="dueDate">
Due date: {{Invoice.DueDate.ToLongDateString()}}
</span>
</p>

| General Information                                                  ||
|:-----------------|---------------------------------------------------:|
| Purchase Amount: | {{Invoice.PurchaseAmount}} {{Invoice.Currency}}    |
| Purchase Price:  | {{Invoice.PurchasePrice}} {{Invoice.Currency}}     |
| Message:         | {{OneRow(Invoice.Message)}}                        |
| Period:          | {{DurationToString(Invoice.Period)}}               |
| Period Interest: | {{Invoice.PeriodInterest}} %                       |
| Installment:     | {{Invoice.Installment}}/{{Invoice.NrInstallments}} |
| Invoice Fee:     | {{Invoice.InvoiceFee}} {{Invoice.Currency}}        |
| Late Fees:       | {{Invoice.LateFees}} {{Invoice.Currency}}          |
| Amount:          | {{Invoice.Amount}} {{Invoice.Currency}}            |
| Amount Paid:     | {{Invoice.AmountPaid}} {{Invoice.Currency}}        |
| Total Amount:    | {{Invoice.TotalAmount}} {{Invoice.Currency}}       |
| **Amount Left**: | **{{Invoice.AmountLeft}} {{Invoice.Currency}}**    |

<button type="button" onclick="ShowDetailedInformation()" class="posButton">Show Details</button>
</div>
<div id="DetailedInformation" style="display:none">

| General Information  ||
|:----------|----------:|
| Is paid | {{Invoice.IsPaid}} |
| Purchase Amount | {{Invoice.PurchaseAmount}} {{Invoice.Currency}} |
| Purchase Price | {{Invoice.PurchasePrice}} {{Invoice.Currency}} |
| Message | {{OneRow(Invoice.Message)}} |
| Period | {{DurationToString(Invoice.Period)}} |
| Period Interest | {{Invoice.PeriodInterest}} % |
| Installment | {{Invoice.Installment}}/{{Invoice.NrInstallments}} |
| Invoice Fee | {{Invoice.InvoiceFee}} {{Invoice.Currency}} |
| Late Fees | {{Invoice.LateFees}} {{Invoice.Currency}} |
| Amount | {{Invoice.Amount}} {{Invoice.Currency}} |
| Amount Paid | {{Invoice.AmountPaid}} {{Invoice.Currency}} |
| Total Amount | {{Invoice.TotalAmount}} {{Invoice.Currency}} |
| **Amount Left** | **{{Invoice.AmountLeft}} {{Invoice.Currency}}** |
| \#Reminders | {{Invoice.NrReminders}} |
| Due Date | {{Invoice.DueDate.ToShortDateString()}} |
| Created | {{Invoice.Created}} |
| Invoice Date | {{Invoice.InvoiceDate.ToShortDateString()}} |

| Personal information about buyer ||
|:----------------|:----------------|
| First name | {{Invoice.FirstName}} |
| Middle name | {{Invoice.MiddleName}} |
| Last name | {{Invoice.LastName}} |
| Personal Number | {{Invoice.PersonalNumber}} |
| Address | {{Invoice.Address}} {{Invoice.Address2}} |
| Area | {{Invoice.Area}} |
| City | {{Invoice.City}} |
| Postal Code | {{Invoice.PostalCode}} |
| Region | {{Invoice.Region}} |
| Country | {{Invoice.Country}} |
| Jid | [{{Invoice.Jid}}](xmpp:{{Invoice.Jid}}) |
| Phone | [{{Invoice.PhoneNumber}}](tel:{{Invoice.PhoneNumber}}) |
| e-Mail | [{{Invoice.EMail}}](mailto:{{Invoice.EMail}}) |

{{if Invoice.IsOrganizational then ]]
| Organizational information about buyer ||
|:----------------------|:----------------|
| Organization | ((Invoice.OrganizationName)) |
| Department | ((Invoice.Department)) |
| Role | ((Invoice.Role)) |
| Organization Number | ((Invoice.OrganizationNumber)) |
| Address | ((Invoice.OrganizationAddress)) ((Invoice.OrganizationAddress2)) |
| Area | ((Invoice.OrganizationArea)) |
| City | ((Invoice.OrganizationCity)) |
| Postal Code | ((Invoice.OrganizationPostalCode)) |
| Region | ((Invoice.OrganizationRegion)) |
| Country | ((Invoice.OrganizationCountry)) |
[[}}

{{
if !empty(Invoice.NeuroCreditsContractId) then
(
	PayUrl:="iotsc:"+Invoice.NeuroCreditsContractId;
	PayUrl:=Waher.IoTGateway.Gateway.GetUrl("/QR/"+UrlEncode(PayUrl));
	]]![Purchase contract]([[;
	]]((PayUrl))[[;
	]])[[
)
}}

<div>
<button type="button" onclick="ShowSimpleInformation()" class="negButton">Hide Details</button>
</div>
</div>

![Checkout](/Checkout.md?Description={{UrlEncode("Payment of invoice "+Str(Nr))}}&Amount={{Invoice.AmountLeft}}&Currency={{Invoice.Currency}}&Country={{Invoice.Country}}&Account={{UrlEncode(Invoice.Account)}}&Callback=TAG.Payments.NeuroCredits.NeuroCreditsService.PaymentReceived&State={{Nr}}&ExcludeProvider=TAG.Payments.NeuroCredits.NeuroCreditsServiceProvider)
