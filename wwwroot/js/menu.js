const serverUrl = 'https://bytech-boards.onrender.com/';
//const serverUrl = 'https://localhost:32779/';

document.addEventListener('DOMContentLoaded', function () {

    let menuItems = document.querySelectorAll('#menu-item');
    let addBoardButton = document.getElementById('add-board-button');

    console.log(menuItems.length);

    for (let i = 0; i < menuItems.length; i++) {

        menuItems[i].onclick = function(event){
            url = url = serverUrl + 'board/index?id=' + event.currentTarget.getAttribute('value');
        
            console.log(url);
            
            window.location.href = url;
        }
    }

    addBoardButton.onmousedown = function (event) {
        window.location.href = serverUrl + 'home/addBoard';
    }
})


