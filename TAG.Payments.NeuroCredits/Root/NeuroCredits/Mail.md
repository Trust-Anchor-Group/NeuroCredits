Title: Mail
Description: Mail settings for the Neuro-Credits™ service.
Date: 2023-11-02
Author: Peter Waher
Master: /Master.md
Javascript: Settings.js
Javascript: /Events.js
Cache-Control: max-age=0, no-cache, no-store
UserVariable: User
Privilege: Admin.Payments.NeuroCredits
Login: /Login.md

========================================================================

Mail settings
================

Invoices can be sent by e-mail. For this to work, you need to assign an e-mail account to the service. Below you can set the parameters
for such an e-mail account, and test it. Once the parameters are configured and work, the service can use these settings to send invoices
automatically to the recipients.

<form action="Mail.md" method="post">
<fieldset>
<legend>SMTP Settings</legend>

{{
if exists(Posted) then
(
	SetSetting("TAG.Payments.NeuroCredits.SMTP.Address",Posted.Address);
	SetSetting("TAG.Payments.NeuroCredits.SMTP.Host",Posted.Host);
	SetSetting("TAG.Payments.NeuroCredits.SMTP.Port",Posted.Port);
	SetSetting("TAG.Payments.NeuroCredits.SMTP.Account",Posted.Account);
	SetSetting("TAG.Payments.NeuroCredits.SMTP.Password",Posted.Password);
	SetSetting("TAG.Payments.NeuroCredits.SMTP.TestRecipient",Posted.TestRecipient);

	TAG.Payments.NeuroCredits.Configuration.MailConfiguration.InvalidateCurrent();

	if !empty(Posted.TestRecipient) then
	(
		Background(
		(
			LogInformation("Sending test message.");
			TAG.Payments.NeuroCredits.NeuroCreditsService.SendTestEMail(Posted.TestRecipient,"Test message",
				"This message was sent as a **test**, to check if the mail parameters provided when configuring the *Neuro-Credits*™ service are correct. You cannot respond to this message.");
		));
	);

	SeeOther("Mail.md");
);
}}

<p>
<label for="Address">E-mail address:</label>  
<input id="Address" name="Address" type="email" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.Address','')}}"/>
</p>

<p>
<label for="Host">SMTP Host:</label>  
<input id="Host" name="Host" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.Host','')}}"/>
</p>

<p>
<label for="Port">SMTP Port:</label>  
<input id="Port" name="Port" type="number" min="1" max="65535" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.Port',25)}}"/>
</p>

<p>
<label for="Account">Account Name:</label>  
<input id="Account" name="Account" type="text" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.Account','')}}"/>
</p>

<p>
<label for="Password">Account Password:</label>  
<input id="Password" name="Password" type="password" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.Password','')}}"/>
</p>

<p>
<label for="TestRecipient">Test Recipient:</label>  
<input id="TestRecipient" name="TestRecipient" type="email" value="{{GetSetting('TAG.Payments.NeuroCredits.SMTP.TestRecipient','')}}"/>
</p>

<button type="submit" class="posButton">Apply & Test</button>
<button type="button" class="negButton" onclick="Close()">Close</button>

**Note**: When applying settings, a test message will be sent to the test recipient provided.

</fieldset>
</form>
