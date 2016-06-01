<?php
/**
 * Created by PhpStorm.
 * User: Jasmine
 * Date: 5/26/2016
 * Time: 9:51 PM
 */
class Game {
    public $host = "";
    public $players = 0;
    public $playerstats = Array();

    function __construct($host, $pc, $p1, $p1s, $p2, $p2s, $p3, $p3s, $p4, $p4s){
        $this->host = $host;
        $this->players = $pc;
        $this->playerstats['player1'] = array(
            'username' =>$p1,
            'score' => $p1s
        );
        $this->playerstats['player2'] = array(
            'username' =>$p2,
            'score' => $p2s
        );
        $this->playerstats['player3'] = array(
            'username' =>$p3,
            'score' => $p3s
        );
        $this->playerstats['player4'] = array(
            'username' =>$p4,
            'score' => $p4s
        );
    }
    function to_str(){
        $text = "<h2><b>".$this->host."'s Game:</b></h2><br/><b>Players:</b> ".(string)$this->players;
        $players = array('player1', 'player2', 'player3', 'player4');
        foreach($players as &$player){
           $text .= "<br><b>Username:</b> ".$this->playerstats[$player]['username']."<br><b>Score:</b> ".$this->playerstats[$player]['score'];
        }
        $text .= "<br><br>";
        return $text;
    }
}
class Player{
    public $username;
    public $kills;
    public $deaths;
    public $wins;
    public $games;
    public $status;

    function __construct ($user, $k, $d, $w, $g, $st){
        $this->username = $user;
        $this->kills = $k;
        $this->deaths = $d;
        $this->wins = $w;
        $this->games = $g;
        $this->status = $st;
    }

    function to_str(){
        $text = "<h2><b>".$this->username."'s Player Info </b></h2>";
        $text .= "<br>Username: ".$this->username."<br>";
        $text .= "Kills: ".$this->kills."<br>";
        $text .= "Wins: ".$this->wins."<br>";
        $text .= "Games: ".$this->games."<br>";
        return $text;
    }
}

?>

