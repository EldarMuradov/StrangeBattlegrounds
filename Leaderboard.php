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


	$sql = "SELECT username, score, level, kills, deaths FROM users ORDER BY score DESC";
    $result = $conn->query($sql);
    if(!empty($result) && $result->num_rows > 0)
    {
		while($row = $result->fetch_assoc())
		{
			echo $row["username"] . " " . $row["score"] . " " . $row["level"] . " " . $row["kills"] ."/" .$row["deaths"] . "<br>";
		}
	}
    $conn->close();
    //echo "Recieved ". $_REQUEST["username"] . " password " .$_REQUEST["password"] . " success!";


?>