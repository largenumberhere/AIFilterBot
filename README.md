<h1>Setup: </h1>
<p>
This project requires aplacca.http to run.
Follow this Get Started (7B) section to install it on your computer <a href="https://github.com/edfletcher/alpaca.http#get-started-7b">https://github.com/edfletcher/alpaca.http#get-started-7b</a>.

Once alpaccahttp is installed and running, run this application. On first run you will be prompted for a discord bot token.
You will need a discord account. Create one if you haven't already and log in. Go to https://discord.com/developers/applications and create a "New Application".
Give it a meaningful name such as "AI Filter Bot"

On the bot Tab, Scroll down to the Privilaged Gateway Intents title.
Turn on Presence Intent, Server Members Intent, Message Content Intent (You probably do not need all of these, I am yet to verify. It shouldn't matter for testing purposes, however).

On the OAuth2 Tab, Go to the sub-tab URL Generator. Click on Bot. In the new box that pops up underneath, tick the box for "Read Messages/View Channels.
Click the blue copy button underneath. Open a new tab in your web browser, paste it in and go. You will be prompted to invite the bot to a server.
Invite it to a discord server that you own for testing.

Go back to the developer portal.
Go to the bot Tab on the left and click Reset Token. The string of letters that shows up is your bot token. You will need to paste this in when prompted by this program.

Ensure you have dotnet 7.0 sdk installed. https://dotnet.microsoft.com/en-us/download/dotnet/7.0 .

</p>
<h1>Running:</h1>
<p>
Run alpaca.http open a powershell in the folder you installed this and run the command "<code>./server --server-address 127.0.0.1 --server-port 1000</code>".

This project was made and run in visual studio, however you don't need it to run this.
In this project's folder (with Program.cs) run the command dotnet run.
You should be greeted with a prompt for the discord token. Paste in the one you got in setup with right-click. 

<h1>Warning:</h1>
This program will save the token in the .\bin\Debug\net7.0\data run directory in a compressed file called "data.file". 
It is not plain text but it is easy to decode with the right knowledge.
Do not data.file to anyone else!
</p>
