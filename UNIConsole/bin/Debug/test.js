const net = require('net');

const PIPE_NAME = "unipipe";
const PIPE_PATH = "\\\\.\\pipe\\" + PIPE_NAME;

let l = console.log;
/**
 * 用于接收的 ipc通道
 */
const server = net.createServer(client=>{
    client.on('data', (data) => {
        l(JSON.parse(data.toString('utf-8')));
    })
});
server.listen("\\\\.\\pipe\\unipipe_get");
/**
 * 用于发送的ipc通道
 */
setInterval(() => {
    const client = net.createConnection(PIPE_PATH, () => {
        //'connect' listener
        l('connected to server!');
        client.write('world!\r\n'); 
        client.end();
    });
    
    client.on('end', () => {
        l('disconnected from server');
    });
}, 3000);