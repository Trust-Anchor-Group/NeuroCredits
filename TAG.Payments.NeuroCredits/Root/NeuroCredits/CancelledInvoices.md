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

| Inv.Nr | Recipient | Pers./Org.Nr | Amount Paid | Currency | Created | Paid | \#Reminders | Installment | Details |
|-------:|:----------|:------------:|------------:|:---------|:-------:|:----:|------------:|:-----------:|--------:|
{{
Invoices:=select 
	top 20 * 
from 
	TAG.Payments.NeuroCredits.Data.Invoice 
where 
	IsPaid=true 
order by 
	InvoiceNumber desc;

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
	]]| <a href="Invoice.md?Nr=((Invoice.InvoiceNumber))" target="_blank">((Invoice.InvoiceNumber))</a> [[;
	]]|
[[
);

if count(Invoices)==20 then ]]

<button id="LoadMoreButton" class='posButton' type="button" onclick='LoadMore(this,20,20,"Cancelled")'>Load More</button>
[[

}}

<button type="button" class="negButton" onclick="Close()">Close</button>
