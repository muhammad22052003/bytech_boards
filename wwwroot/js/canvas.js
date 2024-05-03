//  canvas elements
let canvaBoard = document.getElementById('canvas-board');
let context = canvaBoard.getContext('2d');

//  input brush color
let lineColor = document.getElementById('brush-color');
let strokeStyle = lineColor.value;

//  input brush color
let inputFillColor = document.getElementById('fill-color');
let fillColor = inputFillColor.value;

//  input brush size
let brushSizeInput = document.getElementById('brush-size');
let brushSize = brushSizeInput.value;

let freeModeButton = document.getElementById('free-mode');
let xModeButton = document.getElementById('x-mode');
let yModeButton = document.getElementById('y-mode');
let clearButton = document.getElementById('clear');
let fillModeButton = document.getElementById('fill-mode');
let brushColorInput = document.getElementById('brush-color');
let fillColorInput = document.getElementById('fill-color');
brushSizeInput = document.getElementById('brush-size');
let fontSizeInput = document.getElementById('font-size');
let textInput = document.getElementById('text-field');

const serverUrl = 'https://bytech-boards.onrender.com/';
//const serverUrl = 'https://localhost:32779/';

const sendBoardURL = serverUrl + 'api/setboard';
const loadBoardURL = serverUrl + 'api/getboard';
const checkUpdateForHavenUrl = serverUrl + 'api/getboard';
const menuURL = serverUrl + 'home/index';

let boardId = document.getElementById('board_id').value;
let lastUpdateId = '?';
let boardName = 'board';

function resetColor(event){
    event.target.style.backgroundColor = '#f5f1f1';
}

//  events
lineColor.addEventListener('input', function(){
    strokeStyle = lineColor.value;
});

inputFillColor.addEventListener('input', function(){
    fillColor = inputFillColor.value;
});

brushSizeInput.addEventListener('input', function(){
    if(brushSizeInput.value < 0){
        brushSize = 0;
    }
    else{
        brushSize = Math.min(brushSizeInput.value, 100);
    }

    brushSizeInput.value = brushSize;
});

document.addEventListener('DOMContentLoaded', function () {

    /*updateLoadWorker.postMessage({
        getDataUrl: loadBoardURL
    });*/

    new Promise((async function (resolve) {

        context.fillStyle = 'rgb(255,255,255)';
        context.fillRect(0, 0, canvaBoard.width, canvaBoard.height);
        context.fillColor = fillColor;

        loadCanvasDataFromServer(loadBoardURL + '?id=' + boardId + '&updateId=' + lastUpdateId);
        while (true) {
            loadCanvasDataFromServer(loadBoardURL + '?id=' + boardId + '&updateId=' + lastUpdateId);
            await sleep(2500);
        }
    }));

    setCursor();

    let homeButton = document.getElementById('home-button');
    let trashButton = document.getElementById('trash-button');

    homeButton.onclick = function () {
        console.log('home');
        window.location.href = serverUrl + 'home/index';
    }

    trashButton.onclick = function () {
        console.log('trash');
        window.location.href = serverUrl + 'api/deleteboard?id=' + boardId;
    }
})

function setCursor(){
    canvaBoard.onmousedown = function(event){
        canvaBoard.onmousemove = null;

        canvaBoard.onmouseup = null;
    };

    freeModeButton.hidden = true;
    xModeButton.hidden = true;
    yModeButton.hidden = true;
    fillColorInput.hidden = true;
    brushColorInput.hidden = true;
    brushSizeInput.hidden = true;
    fontSizeInput.hidden = true;
    textInput.hidden = true;
    fillModeButton.hidden = true;

    resetButtonsColor();
    document.getElementById('cursor-tool').style.background = 'rgb(180, 180, 180)';
}

function setPencil(){
    canvaBoard.onmousedown = pencilTool;

    freeModeButton.hidden = false;
    xModeButton.hidden = false;
    yModeButton.hidden = false;
    fillColorInput.hidden = true;
    brushColorInput.hidden = false;
    brushSizeInput.hidden = false;
    fontSizeInput.hidden = true;
    textInput.hidden = true;
    fillModeButton.hidden = true;

    resetButtonsColor();
    document.getElementById('pen-tool').style.background = 'rgb(180, 180, 180)';
}

function setEraser(){
    canvaBoard.onmousedown = eraserTool;

    freeModeButton.hidden = true;
    xModeButton.hidden = true;
    yModeButton.hidden = true;
    fillColorInput.hidden = true;
    brushColorInput.hidden = true;
    brushSizeInput.hidden = false;
    fontSizeInput.hidden = true;
    textInput.hidden = true;
    fillModeButton.hidden = true;

    resetButtonsColor();
    document.getElementById('erase-tool').style.background = 'rgb(180, 180, 180)';
}

function setRectangle(event){
    canvaBoard.onmousedown = createRectangle;

    freeModeButton.hidden = true;
    xModeButton.hidden = true;
    yModeButton.hidden = true;
    fillColorInput.hidden = false;
    brushColorInput.hidden = false;
    brushSizeInput.hidden = false;
    fontSizeInput.hidden = true;
    textInput.hidden = true;
    fillModeButton.hidden = false;

    resetButtonsColor();
    document.getElementById('rec-tool').style.background = 'rgb(180, 180, 180)';
}

function setElipse(){
    canvaBoard.onmousedown = createElipse;

    freeModeButton.hidden = true;
    xModeButton.hidden = true;
    yModeButton.hidden = true;
    fillColorInput.hidden = false;
    brushColorInput.hidden = false;
    brushSizeInput.hidden = false;
    fontSizeInput.hidden = true;
    textInput.hidden = true;
    fillModeButton.hidden = false;

    resetButtonsColor();
    document.getElementById('cir-tool').style.background = 'rgb(180, 180, 180)';
}

function setText(){
    canvaBoard.onmousedown = createText;

    freeModeButton.hidden = true;
    xModeButton.hidden = true;
    yModeButton.hidden = true;
    fillColorInput.hidden = false;
    brushColorInput.hidden = false;
    brushSizeInput.hidden = false;
    fontSizeInput.hidden = false;
    textInput.hidden = false;
    fillModeButton.hidden = false;

    resetButtonsColor();
    document.getElementById('text-tool').style.background = 'rgb(180, 180, 180)';
}

function setXMode(){
    yMode = false;
    xMode = true;
}

function setYMode(){
    yMode = true;
    xMode = false;
}

function setFreeMode(){
    yMode = false;
    xMode = false;
}

function clearAll(){
    context.fillStyle = 'rgb(255,255,255)';
    context.fillRect(0,0, canvaBoard.width, canvaBoard.height);
    context.fillStyle = fillColor;

    sendCanvasDataToServer(sendBoardURL);
}

let fillStyledMode = false;

function setFillStyledMode(){
    if(fillStyledMode) {
        fillStyledMode = false;
    }
    else{
        fillStyledMode = true;
    }
}

// Tools functions

let startX;
let startY;

let xMode = false;
let yMode = false;

function pencilTool (event){
    let startX = event.offsetX;
    let startY = event.offsetY;
    context.lineWidth = brushSize;
    context.strokeStyle = strokeStyle;

    context.beginPath();
    context.moveTo(startX, startY);

    canvaBoard.onmousemove = function(event){
        let x = event.offsetX - 8;
        let y = event.offsetY - 10;
        
        if(xMode){
            y = startY;
        }
        else if(yMode){
            x = startX;
        }

        context.lineTo(x, y);
        context.stroke();
    }

    canvaBoard.onmouseup = function(event){
        canvaBoard.onmousemove = null;

        sendCanvasDataToServer(sendBoardURL);
    }
}

function eraserTool(event){
    let x = event.offsetX;
    let y = event.offsetY;
    context.lineWidth = brushSize;
    context.strokeStyle = 'white';

    context.beginPath();
    context.moveTo(x, y);
    lineStarted = true;

    canvaBoard.onmousemove = async function(event){
        let x = event.offsetX;
        let y = event.offsetY;

        context.lineTo(x, y);
        context.stroke();
    }

    canvaBoard.onmouseup = function(event){
        context.strokeStyle = strokeStyle;
        canvaBoard.onmousemove = null;

        sendCanvasDataToServer(sendBoardURL);
    }
}

function createRectangle(event){
    startX = event.offsetX;
    startY = event.offsetY;

    context.lineWidth = brushSize;
    context.strokeStyle = strokeStyle;
    context.fillStyle = fillColor;

    /*canvaBoard.onmousemove = function(event){
    }*/

    canvaBoard.onmouseup = function(event){
        let x = event.offsetX;
        let y = event.offsetY;

        context.strokeRect(x,y, startX - x, startY - y);
        if(fillStyledMode){
            context.fillRect(x,y, startX - x, startY - y);
        }

        canvaBoard.onmousemove = null;

        sendCanvasDataToServer(sendBoardURL);
    }
}

function createElipse(event){
    startX = event.offsetX;
    startY = event.offsetY;

    context.beginPath();
    context.lineWidth = brushSize;
    context.strokeStyle = strokeStyle;
    context.fillStyle = fillColor;

    canvaBoard.onmouseup = function(event){
        let x = event.offsetX;
        let y = event.offsetY;

        let radius = Math.max(Math.abs(y - startY), Math.abs(x - startX));

        context.ellipse(startX, startY, radius, radius, 0, 0, 2 * Math.PI);
        if(fillStyledMode){
            context.fill();
        }
        context.stroke();

        canvaBoard.onmousemove = null;

        sendCanvasDataToServer(sendBoardURL);
    }
}

function createText(event){
    startX = event.offsetX;
    startY = event.offsetY;

    context.beginPath();
    context.lineWidth = brushSize;
    context.strokeStyle = strokeStyle;
    context.fillStyle = fillColor;
    context.font = fontSizeInput.value + 'px Arial';

    canvaBoard.onmouseup = function(event){
        
        if(fillStyledMode){
            context.fillText(textInput.value, startX, startY);
        }

        // Обведите текст
        context.strokeText(textInput.value, startX, startY);

        canvaBoard.onmousemove = null;

        sendCanvasDataToServer(sendBoardURL);
    }
}

function resetButtonsColor(){
    let buttons = document.getElementsByClassName("tool-button");

    for (let i = 0; i < buttons.length; i++) {
        buttons[i].style.background = '#f5f1f1';
    }
}
 
async function sendCanvasDataToServer(url) {
    const dataUrl = canvaBoard.toDataURL('image/bmp');

    const blob = await(await fetch(dataUrl)).blob();
    
    let formData = new FormData();
    formData.append('image', blob);

    let response;

    response = await fetch(url, {
        method: 'POST',
        headers: {
            'id': boardId,
            'updateId': lastUpdateId,
            'name': boardName
        },
        body: formData,
    });

    if(!response.ok){
        throw new Error('Error In send data to server');
    }

    boardId = response.headers.get('id');
    lastUpdateId = response.headers.get('updateId');
    boardName = response.headers.get('name');
}

async function loadCanvasDataFromServer(url) {

    try {
        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'id': boardId,
                'updateId': lastUpdateId,
                'name': boardName
            }
        });

        if (!response.ok) {
            throw new Error('Data from server not loaded');
        }

        boardId = response.headers.get('id');
        lastUpdateId = response.headers.get('updateId');
        boardName = response.headers.get('name');

        let blob = await response.blob();
        let img = document.createElement('img');
        img.src = URL.createObjectURL(blob);

        img.onload = function () {
            context.drawImage(img, 0, 0);
        }
    } catch (error) {
        //window.location.replace(menuURL);
    }
}

function sleep(ms){
    return new Promise(resolve => setTimeout(resolve, ms));
}
