Setup Guide:

~Google Cloud~
* Set up a vm instance in Google Cloud
* Create a load balancer
* Create a domain which forwards to the frontend IP
* The backend of the load balancer should be the vm instance you created

~FastAPI Server~
* SSH to the vm instance
* upload the files server.py and requirements.txt
* install pip using 'sudo apt-get install pip'
* install requirements using 'sudo python3 -m pip install -r requirements.txt'
* run on your computer: 'tar -czvf mobile.tar.gz mobile' to compress the mobile folder
* upload mobile.tar.gz
* extract using 'tar -xf mobile.tar.gz'

~Running the server~
* run server in debug using 'sudo python3 server.py'
* run server in prod using 'sudo nohup python3 server.py &'
* take down server using 'ps ax | grep server' to get process number and 'sudo kill <NUMBER>' to kill the process
