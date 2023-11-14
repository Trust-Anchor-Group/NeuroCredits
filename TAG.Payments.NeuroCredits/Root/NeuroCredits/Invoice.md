Title: Invoice {{Nr}}
Description: Information about invoice {{Nr}}.
Date: 2023-11-06
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Parameter: Nr
Login: /Login.md

========================================================================

Invoice {{Nr}}
=================

| General Information  ||
|:----------|----------:|
| Account | `{{Invoice:=select top 1 * from TAG.Payments.NeuroCredits.Data.Invoice where InvoiceNumber=Nr;
if !exists(Invoice) then NotFound("Invoice "+Nr+" not found.");
OneRow(s):=empty(s)?"":s.Replace("\r\n","\n").Replace("\r","\n").Replace("&","&amp;").Replace("<","&lt;").Replace(">","&gt;").Replace("\n","<br/>");
Invoice.Account}}` |
| Is paid | {{Invoice.IsPaid}} |
| Purchase Amount | {{Invoice.PurchaseAmount}} {{Invoice.Currency}} |
| Purchase Price | {{Invoice.PurchasePrice}} {{Invoice.Currency}} |
| Amount | {{Invoice.Amount}} {{Invoice.Currency}} |
| Late Fees | {{Invoice.LateFees}} {{Invoice.Currency}} |
| Invoice Fee | {{Invoice.InvoiceFee}} {{Invoice.Currency}} |
| Amount Paid | {{Invoice.AmountPaid}} {{Invoice.Currency}} |
| Total Amount | {{Invoice.TotalAmount}} {{Invoice.Currency}} |
| **Amount Left** | **{{Invoice.AmountLeft}} {{Invoice.Currency}}** |
| \#Reminders | {{Invoice.NrReminders}} |
| Due Date | {{Invoice.DueDate.ToShortDateString()}} |
| Period | {{Invoice.Period}} |
| Period Interest | {{Invoice.PeriodInterest}} % |
| Installment | {{Invoice.Installment}}/{{Invoice.NrInstallments}} |
| Created | {{Invoice.Created}} |
| Paid | {{Invoice.IsPaid ? Invoice.Paid : ""}} |
| Invoice Date | {{Invoice.InvoiceDate.ToShortDateString()}} |
| External Reference | {{OneRow(Invoice.ExternalReference)}} |
| Message | {{OneRow(Invoice.Message)}} |
{{
if !empty(Invoice.NeuroCreditsContractId) then ]]| Purchase contract | <a href="/Contract.md?ID=((Invoice.NeuroCreditsContractId))" target="_blank">((Invoice.NeuroCreditsContractId))</a> |
[[;
if !empty(Invoice.CancellationContractId) then ]]| Cancellation contract | <a href="/Contract.md?ID=((Invoice.CancellationContractId))" target="_blank">((Invoice.CancellationContractId))</a> |
[[;
}}

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
| Agent | {{Invoice.Agent}} |
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

{{if !Invoice.IsPaid then ]]
<button type="button" class="posButton" onclick="ResendInvoice( ((Nr)) )">Resend Invoice</button>
<button type="button" class="posButton" onclick="OpenUrl('PayInvoice.md?Nr=((Nr))&Key=((UrlEncode(Invoice.Key) ))')">Payment Link...</button>
[[}}
<button type="button" class="negButton" onclick="Close()">Close</button>
