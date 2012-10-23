<?php
require_once("includes/functions.php");

// turn debug on if you want to see output instead of downloading the profile

$debug = FALSE;

// define the variables using GET
$username = $_GET['username'];
$password = $_GET['password'];

// Get the profile identifier from the template
$profileID = getProfileID(TEMPLATE);

// Get the cert from Incommon
$certData = getCertificate($username, $password);

// Continue only if we have certificate data
if ($certData) {
	// Build the PKCS12 payload dictionary
	$pkcs12Dict = pkcs12Payload($profileID, $certData);

	// Get the UUID from the PKCS12 payload
	$pkcs12UUID = $pkcs12Dict->get(PAYLOAD_UUID)->getValue();

	// Replace all occurences of $username$ and $pkcs12UUID$ in the template
	// with the actual username and pkcs12UUID
	$mobileconfig = replaceUserAndCert(TEMPLATE, $username, $pkcs12UUID);

	// Add the PKCS12 payload dictionary to the profile
	$mobileconfig->getValue()->get(PAYLOAD_CONTENT)->add($pkcs12Dict);

	// Prepend the user's name to the filename
	$profile = "profiles/" . $username . "." . PROFILE_FILENAME;

	// Save the unsigned profile to the file system
	$mobileconfig->saveXML( $profile );

	// Display the profile
	if ($debug) {
		$xml = file_get_contents( $profile );
		echo 'Profile: <br /><pre>' . xmlpp($xml, TRUE) . '</pre>';
	}

	// Sign the profile
	signProfile($profile);

	// Redirect the user to the signed profile
	if (!$debug) {
		header( 'Location: ' . $profile );
	}
}

?>
