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

$un = $_REQUEST["username"];
$k = $_REQUEST["kills"];
$d = $_REQUEST["deaths"];
$s = $_REQUEST["score"];
if($s > 0)
	$sql = "UPDATE users SET currence = currence + 1, kills = kills + '$k', deaths = deaths + '$d', score = score + '$s' WHERE username = '$un'";
else 
	$sql = "UPDATE users SET kills = kills + '$k', deaths = deaths + '$d', score = score + '$s' WHERE username = '$un'";
$result = $conn->query($sql);
if(!empty($result) && $result->num_rows > 0)
{
	while($row = $result->fetch_assoc())
	{
		echo "Succedeed";
	}
} 
else
{
    echo "Fatal error";
}
$conn->close();

?>
