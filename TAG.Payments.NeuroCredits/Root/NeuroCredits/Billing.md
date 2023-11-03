Title: Billing
Description: Billing settings for the Neuro-Credits™ service.
Date: 2023-11-03
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Billing information
======================

Below you can edit the **billing information** to include in invoices sent by the Neuro-Credits™ service.

<form action="Billing.md" method="post">
<fieldset>
<legend>Invoice Information</legend>

{{
if exists(Posted) then
(
	SetSetting("TAG.Payments.NeuroCredits.Billing.Sender",Posted.Sender);
	SetSetting("TAG.Payments.NeuroCredits.Billing.TaxNr",Posted.TaxNr);
	SetSetting("TAG.Payments.NeuroCredits.Billing.ContactEMail",Posted.ContactEMail);
	SetSetting("TAG.Payments.NeuroCredits.Billing.ContactPhone",Posted.ContactPhone);
	SetSetting("TAG.Payments.NeuroCredits.Billing.BankAccount",Posted.BankAccount);
	SetSetting("TAG.Payments.NeuroCredits.Billing.Iban",Posted.Iban);
	SetSetting("TAG.Payments.NeuroCredits.Billing.BankAccountName",Posted.BankAccountName);
	SetSetting("TAG.Payments.NeuroCredits.Billing.BankIdentifier",Posted.BankIdentifier);
	SetSetting("TAG.Payments.NeuroCredits.Billing.BankName",Posted.BankName);
	SetSetting("TAG.Payments.NeuroCredits.Billing.BranchAddress",Posted.BranchAddress);

	TAG.Payments.NeuroCredits.BillingConfiguration.InvalidateCurrent();

	SeeOther("Billing.md");
);
}}

<p>
<label for="Sender">Name of sender:</label>  
<input id="Sender" name="Sender" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.Sender','')}}"/>
</p>

<p>
<label for="TaxNr">Tax (or VAT) number:</label>  
<input id="TaxNr" name="TaxNr" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.TaxNr','')}}"/>
</p>

<p>
<label for="ContactEMail">Contact e-mail:</label>  
<input id="ContactEMail" name="ContactEMail" required type="email" value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.ContactEMail','')}}"/>
</p>

<p>
<label for="ContactPhone">Contact phone:</label>  
<input id="ContactPhone" name="ContactPhone" type="tel" value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.ContactPhone','')}}"/>
</p>

<p>
<label for="BankAccount">Bank account: (local, bank or national numbering system)</label>  
<input id="BankAccount" name="BankAccount" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.BankAccount','')}}"/>
</p>

<p>
<label for="Iban">IBAN number:</label>  
<input id="Iban" name="Iban" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.Iban','')}}"/>
</p>

<p>
<label for="BankAccountName">Account holder's name:</label>  
<input id="BankAccountName" name="BankAccountName" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.BankAccountName','')}}"/>
</p>

<p>
<label for="BankIdentifier">Bank identifier (BIC or SWIFT code):</label>  
<input id="BankIdentifier" name="BankIdentifier" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.BankIdentifier','')}}"/>
</p>

<p>
<label for="BankName">Bank name:</label>  
<input id="BankName" name="BankName" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.BankName','')}}"/>
</p>

<p>
<label for="BranchAddress">Bank branch address:</label>  
<input id="BranchAddress" name="BranchAddress" type="text" required value="{{GetSetting('TAG.Payments.NeuroCredits.Billing.BranchAddress','')}}"/>
</p>

<button type="submit" class="posButton">Apply</button>
<button type="button" class="negButton" onclick="Close()">Close</button>
</fieldset>
</form>
