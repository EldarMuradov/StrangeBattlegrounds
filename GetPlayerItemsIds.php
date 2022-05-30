<?php
    
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "unitybackenddb";
$conn = new mysqli($servername, $username, $password, $dbname);
$kd;
$rank;

if($conn->connect_error)
{ 
	die("Connection failed: " . $conn->connect_error);
}

if($_REQUEST["userId"])
{
$sql = "SELECT itemId FROM usersitems WHERE userId = '" .$_REQUEST["userId"] . "'";
$result = $conn->query($sql);
if(!empty($result) && $result->num_rows > 0)
{
	$rows = array();
	while($row = $result->fetch_assoc())
	{
		$rows[] = $row;
	} 
	echo json_encode($rows);
} 
else
{
    echo "0";
}
}
$conn->close();

?>
