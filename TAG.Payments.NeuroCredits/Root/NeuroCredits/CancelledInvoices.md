Title: Cancelled Invoices
Description: Shows cancelled invoices
Date: 2023-10-31
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Cancelled invoices
=====================

Below you can review cancelled invoices, and if they have been paid, revoked, aborted, etc.

| Inv.Nr | Recipient | Pers./Org.Nr | Amount Paid | Currency | Created | Paid | \#Reminders | Installment | Contract |
|-------:|:----------|:------------:|------------:|:---------|:-------:|:----:|------------:|:-----------:|:---------|
{{
Invoices:=select * from TAG.Payments.NeuroCredits.Data.Invoice where IsPaid=true order by InvoiceNumber;
foreach Invoice in Invoices do
(
	]]| ((Invoice.InvoiceNumber)) [[;
	]]| ((Invoice.IsOrganizational ? Invoice.OrganizationName : Invoice.PersonalName)) [[;
	]]| ((Invoice.IsOrganizational ? Invoice.OrganizationNumber : Invoice.PersonalNumber)) [[;
	]]| ((Invoice.AmountPaid)) [[;
	]]| ((Invoice.Currency)) [[;
	]]| ((Invoice.Created)) [[;
	]]| ((Invoice.Paid)) [[;
	]]| ((Invoice.NrReminders)) [[;
	]]| ((Invoice.Installment))/((Invoice.NrInstallments)) [[;
	]]| ((empty(Invoice.CancellationContractId)?"":"[Contract](/Contract.md?ID="+Invoice.CancellationContractId+")")) [[;
	]]|
[[
)
}}

<button type="button" class="negButton" onclick="Close()">Close</button>
