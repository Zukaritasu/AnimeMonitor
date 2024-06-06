const fs = require('fs');
const { File } = require('megajs')
const net = require('net');
const { exit } = require('process');
const path = require('path');

const ResponseStatus = {
    SUCCEEDED: 0,
    FAILED: 1,
    UNAUTHORIZED: 2,
    DOWNLOAD_PROGRESS: 3
}

function createReport(error) {
    const formattedDate = new Date().toISOString().replace(/[-T:]/g, '_').slice(0, -5);
    const filePath = path.join(__dirname, 'reports', `report_${formattedDate}.txt`);
    if (!fs.existsSync('./reports'))
        fs.mkdirSync('./reports')
    fs.writeFileSync(filePath, error !== null && error !== undefined ? `${error}` : 
        'An unknown error has occurred');
}

async function sendFileInfo(client, link) {
    let response;
    try {
        const file = File.fromURL(link)
        await file.loadAttributes()
        response = JSON.stringify({
            status: ResponseStatus.SUCCEEDED,
            data: {
                filename: file.name,
                size: file.size
            }
        })
    } catch (error) {
        createReport(error)
        response = JSON.stringify({
            status: ResponseStatus.FAILED,
            message: error.message
        })
    }

    client.write(response)
}

async function download(client, epInfo) {
    try {
        const file = File.fromURL(epInfo.link)
        const filePath = path.join(epInfo.output, epInfo.filename.replace(/\.mp4$/, ""))
        let attempts = 4
        while (true) {
            const result = await new Promise((resolve, _reject) => {
                let stream = null
                let writableStream = null
                if (!fs.existsSync(filePath)) {
                    stream = file.download()
                    writableStream = fs.createWriteStream(filePath)
                } else {
                    const start = fs.statSync(filePath).size
                    stream = file.download({ start })
                    writableStream = fs.createWriteStream(filePath, {
                        flags: 'r+',
                        start
                    })
                }

                stream.on('error', (error) => {
                    if (error !== null && error !== undefined && 'message' in error) {
                        client.write(JSON.stringify({
                            status: ResponseStatus.FAILED,
                            stop: false,
                            message: error.message
                        }))
                    }
                    resolve(false)
                })
                stream.on('end', () => {
                    try {
                        fs.rename(filePath, filePath + '.mp4', (err) => {
                            if (!err) throw err
                        })
                        client.write(JSON.stringify({ status: ResponseStatus.SUCCEEDED }))
                    } catch (error) {
                        createReport(error)
                        client.write(JSON.stringify({
                            status: ResponseStatus.FAILED,
                            stop: true,
                            message: error.message
                        }))
                    }
                    resolve(true)
                })
                stream.on('progress', info => {
                    client.write(JSON.stringify({
                        status: ResponseStatus.DOWNLOAD_PROGRESS,
                        loaded: info.bytesLoaded,
                        total: info.bytesTotal
                    }))
                })
                stream.pipe(writableStream)
            })

            if (attempts === 1) {
                client.write(JSON.stringify({
                    status: ResponseStatus.FAILED,
                    stop: true,
                    message: `The number of retries to download the file has been exceeded`
                }))
                break
            }

            if (result) {
                break
            }
            --attempts
        }
    } catch (error) {
        createReport(error)
    }
}

if (process.argv.length == 3) {
    if (isNaN(process.argv[2])) {
        createReport(new Error('The argument is not a number'))
        exit(1);
    } else {
        const client = new net.Socket();
        client.connect(parseInt(process.argv[2]), '127.0.0.1', () => {
            console.log('Connected to server');
        });

        client.on('data', async (data) => {
            const _data = JSON.parse(data);
            if (_data.command === '--fileinfo') {
                await sendFileInfo(client, _data.link)
            } else if (_data.command === '--download') {
                try {
                    await download(client, _data)
                } catch (error) {
                    createReport(error)
                    client.write(JSON.stringify({ 
                        status: ResponseStatus.FAILED, 
                        message: error.message
                    }))
                }
            }
        });

        client.on('close', () => {
            console.log('Connection closed');
        });
    }
}

/*sendFileInfo({ write: (obj) => {
    console.log(obj)
} }, 'https://mega.nz/file/ZOkwHCoR#vFEQMI76u9KAHjN_49pgSoymIPJg-lND6ERzx_UluIo')*/

/*download({ write: (obj) => {
    console.log(obj)
} }, {
    link: 'https://mega.nz/file/ZOkwHCoR#vFEQMI76u9KAHjN_49pgSoymIPJg-lND6ERzx_UluIo',
    output: __dirname,
    filename: 'video.mp4'
})*/
