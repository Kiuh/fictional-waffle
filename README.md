# fictional-waffle

## Overview

Course project of third year hight school.   
Includes:
- Game client on unity
- Game authorization service on asp.net
- Statistic service on asp.net
- Room manager on asp.net
- Admin authorization service on asp.net
- Admin client on maui

## Comments

All project configured to be deplayed on docker with 7 containers total.  
Room manager can deploy container with unity server.  
Game client uses unity`s bitesize 2d space shooter sample with netcode.  
Game authorization service uses secrets for configuring, can send email and supports jwt tokens.
