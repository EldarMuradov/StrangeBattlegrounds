<?php
    
$servername = "localhost";
$username = "root";
$password = "";
$dbname = "unitybackenddb";
$conn = new mysqli($servername, $username, $password, $dbname);

if($conn->connect_error)
{ 
	die("Connection failed: " . $conn->connect_error);
}

if($_REQUEST["username"])
{

$sql = "SELECT currence FROM users WHERE username = '" . $_REQUEST["username"] . "'";
$result = $conn->query($sql);
if(!empty($result) && $result->num_rows > 0)
{
	while($row = $result->fetch_assoc())
	{
		
		echo  $row["currence"];
	} 
} 
else
{
    echo "Fatal error";
}
$conn->close();
}


?>
