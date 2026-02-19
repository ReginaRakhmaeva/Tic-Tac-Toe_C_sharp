let currentGameId = null;
let gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
let isPlayerTurn = true;
let gameFinished = false;
let gameMode = null; // 'computer' or 'player'
let currentPlayerSymbol = 1; // 1 = X, 2 = O
let currentPlayerId = null;
let player1Id = null;
let player2Id = null;
let refreshInterval = null;

// Получение заголовков авторизации
function getAuthHeaders() {
    const headers = {
        'Content-Type': 'application/json'
    };
    
    const authCredentials = localStorage.getItem('authCredentials');
    if (authCredentials) {
        headers['Authorization'] = 'Basic ' + authCredentials;
    }
    
    return headers;
}

// Проверка авторизации
function checkAuth() {
    const authCredentials = localStorage.getItem('authCredentials');
    if (!authCredentials) {
        document.getElementById('authWarning').style.display = 'block';
        return false;
    }
    document.getElementById('authWarning').style.display = 'none';
    return true;
}

// Показать меню выбора режима
function showGameModeSelection() {
    document.getElementById('gameModeSelection').style.display = 'block';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'none';
    gameMode = null;
    currentGameId = null;
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
}

// Показать режим игры с компьютером
function showComputerMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'block';
    document.getElementById('playerGameContainer').style.display = 'none';
    gameMode = 'computer';
    currentGameId = null;
    gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    
    // Скрываем игровое поле до выбора кто начинает
    document.getElementById('gameBoard').style.display = 'none';
    document.getElementById('gameStatus').textContent = 'Выберите, кто начинает игру';
    document.getElementById('gameId').style.display = 'none';
    document.getElementById('errorMessage').style.display = 'none';
    
    // Сбрасываем состояние игры
    isPlayerTurn = false;
    gameFinished = false;
    clearBoard('gameBoard');
}

// Показать режим игры с игроком
function showPlayerMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'block';
    document.getElementById('playerGameBoard').style.display = 'none';
    document.getElementById('availableGamesList').style.display = 'block'; // Показываем список
    
    // Показываем кнопки управления
    document.getElementById('createGameBtn').style.display = 'inline-block';
    document.getElementById('refreshGamesBtn').style.display = 'inline-block';
    document.getElementById('backToMenuBtn2').style.display = 'inline-block';
    
    gameMode = 'player';
    currentGameId = null;
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
    loadAvailableGames();
}

// Загрузка списка доступных игр
async function loadAvailableGames() {
    try {
        // Показываем список, если он был скрыт
        document.getElementById('availableGamesList').style.display = 'block';
        document.getElementById('playerGameBoard').style.display = 'none';
        
        const response = await fetch('/game/available', {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            throw new Error('Ошибка при загрузке списка игр');
        }

        const games = await response.json();
        displayAvailableGames(games);
    } catch (error) {
        console.error('Ошибка при загрузке игр:', error);
        document.getElementById('gamesList').innerHTML = 
            '<p class="text-danger">Ошибка при загрузке игр: ' + error.message + '</p>';
    }
}

// Отображение списка доступных игр
function displayAvailableGames(games) {
    const gamesList = document.getElementById('gamesList');
    
    if (!games || games.length === 0) {
        gamesList.innerHTML = '<p class="text-muted">Нет доступных игр. Создайте новую игру!</p>';
        return;
    }

    gamesList.innerHTML = games.map(game => `
        <div class="list-group-item d-flex justify-content-between align-items-center">
            <div>
                <strong>Игра #${game.id.substring(0, 8)}</strong>
                <br>
                <small class="text-muted">Создана игроком</small>
            </div>
            <button class="btn btn-primary btn-sm" onclick="joinGame('${game.id}')">
                Присоединиться
            </button>
        </div>
    `).join('');
}

// Создание новой игры
async function createPlayerGame() {
    try {
        const response = await fetch('/game', {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                gameType: 'player'
            })
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            const error = await response.json();
            throw new Error(error.message || 'Ошибка при создании игры');
        }

        const game = await response.json();
        
        if (game.status === 'WaitingForPlayers') {
            // Игра создана, но еще нет второго игрока
            // Показываем сообщение и обновляем список
            alert('Игра создана! Ожидайте присоединения второго игрока.');
            
            // Начинаем опрос игры, чтобы узнать, когда присоединится второй игрок
            currentGameId = game.id;
            player1Id = game.player1Id;
            player2Id = game.player2Id;
            currentPlayerId = game.currentPlayerId;
            
            // Устанавливаем правильный символ для создателя игры (Player1 = X)
            const userId = localStorage.getItem('userId');
            if (userId && userId === player1Id) {
                currentPlayerSymbol = 1; // X
            }
            
            // Сбрасываем доску из состояния игры
            if (game.board && game.board.board) {
                gameBoard = game.board.board.map(row => [...row]); // Глубокая копия
            } else {
                gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]]; // Пустая доска
            }
            
            // Показываем игровое поле для ожидания
            document.getElementById('availableGamesList').style.display = 'none';
            document.getElementById('playerGameBoard').style.display = 'block';
            
            // Скрываем кнопки управления при создании игры
            document.getElementById('createGameBtn').style.display = 'none';
            document.getElementById('refreshGamesBtn').style.display = 'none';
            document.getElementById('backToMenuBtn2').style.display = 'none';
            
            // Убеждаемся, что обработчик события привязан к кнопке выхода
            const leaveGameBtn = document.getElementById('leaveGameBtn');
            if (leaveGameBtn) {
                // Удаляем старый обработчик, если есть
                leaveGameBtn.replaceWith(leaveGameBtn.cloneNode(true));
                // Добавляем новый обработчик
                document.getElementById('leaveGameBtn').addEventListener('click', leavePlayerGame);
            }
            
            updatePlayerBoard(gameBoard);
            updatePlayerGameInfo(game);
            
            if (refreshInterval) {
                clearInterval(refreshInterval);
            }
            refreshInterval = setInterval(pollPlayerGame, 2000);
        } else {
            // Второй игрок уже присоединился или игра началась
            startPlayerGame(game);
        }
    } catch (error) {
        console.error('Ошибка при создании игры:', error);
        alert('Ошибка: ' + error.message);
    }
}

// Присоединение к игре
async function joinGame(gameId) {
    try {
        const response = await fetch(`/game/${gameId}/join`, {
            method: 'POST',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            const error = await response.json();
            throw new Error(error.message || 'Ошибка при присоединении к игре');
        }

        const game = await response.json();
        startPlayerGame(game);
    } catch (error) {
        console.error('Ошибка при присоединении:', error);
        alert('Ошибка: ' + error.message);
    }
}

// Начало игры с игроком
function startPlayerGame(game) {
    currentGameId = game.id;
    player1Id = game.player1Id;
    player2Id = game.player2Id;
    
    const userId = localStorage.getItem('userId');
    if (userId) {
        if (userId === player1Id) {
            currentPlayerSymbol = 1; // X
        } else if (userId === player2Id) {
            currentPlayerSymbol = 2; // O
        }
    }
    
    currentPlayerId = game.currentPlayerId;
    
    // Сбрасываем доску из состояния игры (важно для новой игры)
    if (game.board && game.board.board) {
        gameBoard = game.board.board.map(row => [...row]); // Глубокая копия
    } else {
        gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]]; // Пустая доска
    }
    
    document.getElementById('availableGamesList').style.display = 'none';
    document.getElementById('playerGameBoard').style.display = 'block';
    
    // Скрываем кнопки управления при начале игры
    document.getElementById('createGameBtn').style.display = 'none';
    document.getElementById('refreshGamesBtn').style.display = 'none';
    document.getElementById('backToMenuBtn2').style.display = 'none';
    
    // Убеждаемся, что обработчик события привязан к кнопке выхода
    const leaveGameBtn = document.getElementById('leaveGameBtn');
    if (leaveGameBtn) {
        // Удаляем старый обработчик, если есть
        leaveGameBtn.replaceWith(leaveGameBtn.cloneNode(true));
        // Добавляем новый обработчик
        document.getElementById('leaveGameBtn').addEventListener('click', leavePlayerGame);
    }
    
    updatePlayerGameInfo(game);
    updatePlayerBoard(gameBoard);
    
    // Начинаем опрос состояния игры
    if (refreshInterval) {
        clearInterval(refreshInterval);
    }
    refreshInterval = setInterval(pollPlayerGame, 2000); // Опрос каждые 2 секунды
}

// Обновление информации об игре
function updatePlayerGameInfo(game) {
    const userId = localStorage.getItem('userId');
    if (!userId) return;
    
    const isMyTurn = game.currentPlayerId && game.currentPlayerId === userId;
    
    let statusText = '';
    if (game.status === 'WaitingForPlayers') {
        statusText = 'Ожидание второго игрока...';
    } else if (game.status === 'PlayerTurn') {
        // Проверяем, не вышел ли соперник (если один из игроков null)
        if (game.player1Id == null || game.player2Id == null) {
            statusText = 'Ваш соперник вышел из игры';
        } else {
            statusText = isMyTurn ? 'Ваш ход!' : 'Ход соперника...';
        }
    } else if (game.status === 'PlayerWins') {
        if (game.winnerId === userId) {
            statusText = 'Вы выиграли!';
        } else {
            statusText = 'Вы проиграли!';
        }
    } else if (game.status === 'Draw') {
        statusText = 'Ничья!';
    } else if (game.status === 'PlayerLeft') {
        statusText = 'Ваш соперник вышел из игры';
    }
    
    document.getElementById('playerGameStatus').textContent = statusText;
    document.getElementById('playerGameId').textContent = 'ID игры: ' + game.id.substring(0, 8);
    
    let playerInfo = '';
    if (player1Id === userId) {
        playerInfo = 'Вы играете за X';
    } else if (player2Id === userId) {
        playerInfo = 'Вы играете за O';
    }
    document.getElementById('playerInfo').textContent = playerInfo;
}

// Опрос состояния игры (для PvP)
async function pollPlayerGame() {
    if (!currentGameId) return;
    
    try {
        const response = await fetch(`/game/${currentGameId}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 404) {
                // Игра не найдена - возможно соперник вышел и удалил игру
                const userId = localStorage.getItem('userId');
                if (userId && currentGameId) {
                    // Останавливаем опрос
                    if (refreshInterval) {
                        clearInterval(refreshInterval);
                        refreshInterval = null;
                    }
                    // Показываем сообщение
                    document.getElementById('playerGameStatus').textContent = 'Ваш соперник вышел из игры';
                    // Возвращаемся к списку через несколько секунд
                    setTimeout(() => {
                        document.getElementById('playerGameBoard').style.display = 'none';
                        document.getElementById('availableGamesList').style.display = 'block';
                        document.getElementById('createGameBtn').style.display = 'inline-block';
                        document.getElementById('refreshGamesBtn').style.display = 'inline-block';
                        document.getElementById('backToMenuBtn2').style.display = 'inline-block';
                        currentGameId = null;
                        loadAvailableGames();
                    }, 3000);
                }
            }
            return;
        }

        const game = await response.json();
        
        // Проверяем статус PlayerLeft
        if (game.status === 'PlayerLeft') {
            // Останавливаем опрос
            if (refreshInterval) {
                clearInterval(refreshInterval);
                refreshInterval = null;
            }
            // Обновляем информацию об игре (покажет сообщение о выходе соперника)
            updatePlayerGameInfo(game);
            // Возвращаемся к списку через несколько секунд
            setTimeout(() => {
                document.getElementById('playerGameBoard').style.display = 'none';
                document.getElementById('availableGamesList').style.display = 'block';
                document.getElementById('createGameBtn').style.display = 'inline-block';
                document.getElementById('refreshGamesBtn').style.display = 'inline-block';
                document.getElementById('backToMenuBtn2').style.display = 'inline-block';
                currentGameId = null;
                loadAvailableGames();
            }, 3000);
            return;
        }
        
        // Если игра еще ожидает второго игрока, не обновляем доску
        if (game.status === 'WaitingForPlayers') {
            updatePlayerGameInfo(game);
            return;
        }
        
        // Обновляем информацию об игре
        currentPlayerId = game.currentPlayerId;
        player1Id = game.player1Id;
        player2Id = game.player2Id;
        
        updatePlayerGameInfo(game);
        if (game.board && game.board.board) {
            updatePlayerBoard(game.board.board);
        }
        
        // Если игра завершена, останавливаем опрос и возвращаем в меню
        if (game.status === 'PlayerWins' || game.status === 'Draw') {
            if (refreshInterval) {
                clearInterval(refreshInterval);
                refreshInterval = null;
            }
            // Показываем финальное сообщение и возвращаем в меню через несколько секунд
            setTimeout(() => {
                // Останавливаем опрос если еще не остановлен
                if (refreshInterval) {
                    clearInterval(refreshInterval);
                    refreshInterval = null;
                }
                document.getElementById('playerGameBoard').style.display = 'none';
                document.getElementById('availableGamesList').style.display = 'block';
                document.getElementById('createGameBtn').style.display = 'inline-block';
                document.getElementById('refreshGamesBtn').style.display = 'inline-block';
                document.getElementById('backToMenuBtn2').style.display = 'inline-block';
                // Сбрасываем все переменные состояния
                currentGameId = null;
                player1Id = null;
                player2Id = null;
                currentPlayerId = null;
                gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
                loadAvailableGames();
            }, 5000); // 5 секунд чтобы игроки увидели результат
        }
    } catch (error) {
        console.error('Ошибка при опросе игры:', error);
    }
}

// Обновление доски для PvP
function updatePlayerBoard(board) {
    gameBoard = board;
    const boardElement = document.getElementById('playerGameBoardElement');
    boardElement.querySelectorAll('.cell').forEach(cell => {
        const row = parseInt(cell.dataset.row);
        const col = parseInt(cell.dataset.col);
        const value = board[row][col];
        
        cell.textContent = '';
        cell.classList.remove('x', 'o');
        
        if (value === 1) {
            cell.textContent = 'X';
            cell.classList.add('x');
        } else if (value === 2) {
            cell.textContent = 'O';
            cell.classList.add('o');
        }
    });
}

// Инициализация игры с компьютером
async function initializeGame(computerFirst) {
    currentGameId = generateUUID();
    gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    isPlayerTurn = !computerFirst;
    gameFinished = false;
    currentPlayerSymbol = 1; // Игрок всегда X против компьютера
    
    document.getElementById('gameBoard').style.display = 'inline-block';
    document.getElementById('gameId').textContent = 'ID игры: ' + currentGameId.substring(0, 8);
    document.getElementById('gameId').style.display = 'block';
    
    if (computerFirst) {
        document.getElementById('gameStatus').textContent = 'Ход компьютера...';
    } else {
        document.getElementById('gameStatus').textContent = 'Ваш ход (X)';
    }
    
    document.getElementById('errorMessage').style.display = 'none';
    
    clearBoard('gameBoard');
    
    try {
        const response = await fetch('/game', {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                gameType: 'computer',
                firstMove: computerFirst ? 'computer' : 'player'
            })
        });
        
        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            throw new Error('Ошибка при создании игры');
        }
        
        const gameResponse = await response.json();
        currentGameId = gameResponse.id;
        
        if (gameResponse && gameResponse.board && gameResponse.board.board) {
            gameBoard = gameResponse.board.board;
            updateBoard('gameBoard');
            
            handleGameStatus(gameResponse.status, 'gameStatus');
            
            if (!gameFinished) {
                isPlayerTurn = true;
                document.getElementById('gameStatus').textContent = 'Ваш ход (X)';
            }
        }
    } catch (error) {
        console.error('Ошибка при инициализации игры:', error);
        document.getElementById('errorMessage').textContent = 'Ошибка: ' + error.message;
        document.getElementById('errorMessage').style.display = 'block';
        if (!computerFirst) {
            isPlayerTurn = true;
            document.getElementById('gameStatus').textContent = 'Ваш ход (X)';
        }
    }
}

// Ход в игре с компьютером
async function makeComputerMove() {
    try {
        document.getElementById('gameStatus').textContent = 'Ход компьютера...';
        document.getElementById('errorMessage').style.display = 'none';
        
        const response = await fetch(`/game/${currentGameId}`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                id: currentGameId,
                board: {
                    board: gameBoard
                }
            })
        });

        const responseText = await response.text();
        
        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            let errorMessage = 'Ошибка сервера';
            try {
                const error = JSON.parse(responseText);
                errorMessage = error.message || error.details || 'Ошибка сервера';
            } catch (e) {
                errorMessage = responseText || 'Ошибка сервера';
            }
            throw new Error(errorMessage);
        }

        const gameResponse = JSON.parse(responseText);
        
        if (gameResponse && gameResponse.board && gameResponse.board.board) {
            gameBoard = gameResponse.board.board;
            updateBoard('gameBoard');
        }
        
        handleGameStatus(gameResponse.status, 'gameStatus');
        
        if (!gameFinished) {
            isPlayerTurn = true;
            document.getElementById('gameStatus').textContent = 'Ваш ход (X)';
        } else {
            isPlayerTurn = false;
        }
        
    } catch (error) {
        document.getElementById('errorMessage').textContent = 'Ошибка: ' + error.message;
        document.getElementById('errorMessage').style.display = 'block';
        isPlayerTurn = true;
        document.getElementById('gameStatus').textContent = 'Ваш ход (X)';
    }
}

// Ход в игре с игроком
async function makePlayerMove() {
    try {
        const userId = localStorage.getItem('userId');
        if (!userId) return;
        
        const isMyTurn = currentPlayerId && currentPlayerId === userId;
        
        if (!isMyTurn) {
            return;
        }
        
        document.getElementById('playerGameStatus').textContent = 'Отправка хода...';
        document.getElementById('playerGameErrorMessage').style.display = 'none';
        
        const response = await fetch(`/game/${currentGameId}`, {
            method: 'POST',
            headers: getAuthHeaders(),
            body: JSON.stringify({
                id: currentGameId,
                board: {
                    board: gameBoard
                }
            })
        });

        const responseText = await response.text();
        
        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            let errorMessage = 'Ошибка сервера';
            try {
                const error = JSON.parse(responseText);
                errorMessage = error.message || error.details || 'Ошибка сервера';
                
                // Если соперник вышел из игры, обрабатываем это специально
                if (errorMessage.includes('opponent has left') || errorMessage.includes('соперник')) {
                    // Останавливаем опрос
                    if (refreshInterval) {
                        clearInterval(refreshInterval);
                        refreshInterval = null;
                    }
                    // Показываем сообщение
                    document.getElementById('playerGameStatus').textContent = 'Ваш соперник вышел из игры';
                    // Возвращаемся к списку через несколько секунд
                    setTimeout(() => {
                        document.getElementById('playerGameBoard').style.display = 'none';
                        document.getElementById('availableGamesList').style.display = 'block';
                        document.getElementById('createGameBtn').style.display = 'inline-block';
                        document.getElementById('refreshGamesBtn').style.display = 'inline-block';
                        document.getElementById('backToMenuBtn2').style.display = 'inline-block';
                        currentGameId = null;
                        loadAvailableGames();
                    }, 3000);
                    return;
                }
            } catch (e) {
                errorMessage = responseText || 'Ошибка сервера';
            }
            throw new Error(errorMessage);
        }

        const gameResponse = JSON.parse(responseText);
        
        if (gameResponse && gameResponse.board && gameResponse.board.board) {
            gameBoard = gameResponse.board.board;
            updatePlayerBoard(gameBoard);
        }
        
        // Обновляем текущего игрока
        currentPlayerId = gameResponse.currentPlayerId;
        player1Id = gameResponse.player1Id;
        player2Id = gameResponse.player2Id;
        
        updatePlayerGameInfo(gameResponse);
        
    } catch (error) {
        document.getElementById('playerGameErrorMessage').textContent = 'Ошибка: ' + error.message;
        document.getElementById('playerGameErrorMessage').style.display = 'block';
    }
}

function updateBoard(boardId) {
    const boardElement = document.getElementById(boardId);
    boardElement.querySelectorAll('.cell').forEach(cell => {
        const row = parseInt(cell.dataset.row);
        const col = parseInt(cell.dataset.col);
        const value = gameBoard[row][col];
        
        cell.textContent = '';
        cell.classList.remove('x', 'o');
        
        if (value === 1) {
            cell.textContent = 'X';
            cell.classList.add('x');
        } else if (value === 2) {
            cell.textContent = 'O';
            cell.classList.add('o');
        }
    });
}

function clearBoard(boardId) {
    const boardElement = document.getElementById(boardId);
    boardElement.querySelectorAll('.cell').forEach(cell => {
        cell.textContent = '';
        cell.classList.remove('x', 'o');
    });
}

function handleGameStatus(status, statusElementId) {
    if (!status || status === 'InProgress' || status === 'PlayerTurn') {
        gameFinished = false;
        return;
    }
    
    gameFinished = true;
    isPlayerTurn = false;
    
    let message = '';
    switch (status) {
        case 'PlayerWins':
            const userId = localStorage.getItem('userId');
            if (userId) {
                // Проверяем, выиграл ли текущий игрок
                // Это будет обработано в updatePlayerGameInfo для PvP
                message = 'Игра завершена!';
            } else {
                message = 'Игра завершена!';
            }
            break;
        case 'Draw':
            message = 'Ничья!';
            break;
        default:
            message = 'Игра завершена';
    }
    
    const statusElement = document.getElementById(statusElementId);
    if (statusElement) {
        statusElement.textContent = message;
    }
}

// Выход из игры с игроком
async function leavePlayerGame() {
    if (!currentGameId) {
        return;
    }

    if (!confirm('Вы уверены, что хотите выйти из игры?')) {
        return;
    }

    try {
        const response = await fetch(`/game/${currentGameId}/leave`, {
            method: 'POST',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('userId');
                if (typeof updateNavigation === 'function') {
                    updateNavigation();
                }
                window.location.href = '/Login';
                return;
            }
            
            // Если игра уже удалена (404), просто возвращаем в меню
            if (response.status === 404) {
                // Останавливаем опрос
                if (refreshInterval) {
                    clearInterval(refreshInterval);
                    refreshInterval = null;
                }
                // Возвращаемся к списку игр
                document.getElementById('playerGameBoard').style.display = 'none';
                document.getElementById('availableGamesList').style.display = 'block';
                document.getElementById('createGameBtn').style.display = 'inline-block';
                document.getElementById('refreshGamesBtn').style.display = 'inline-block';
                document.getElementById('backToMenuBtn2').style.display = 'inline-block';
                // Сбрасываем все переменные состояния
                currentGameId = null;
                player1Id = null;
                player2Id = null;
                currentPlayerId = null;
                gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
                loadAvailableGames();
                return;
            }
            
            const error = await response.json();
            throw new Error(error.message || 'Ошибка при выходе из игры');
        }

        // Останавливаем опрос игры
        if (refreshInterval) {
            clearInterval(refreshInterval);
            refreshInterval = null;
        }

        // Сбрасываем состояние
        currentGameId = null;
        player1Id = null;
        player2Id = null;
        currentPlayerId = null;
        gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];

        // Возвращаемся к списку игр
        document.getElementById('playerGameBoard').style.display = 'none';
        document.getElementById('availableGamesList').style.display = 'block';
        
        // Показываем кнопки управления
        document.getElementById('createGameBtn').style.display = 'inline-block';
        document.getElementById('refreshGamesBtn').style.display = 'inline-block';
        document.getElementById('backToMenuBtn2').style.display = 'inline-block';
        
        // Обновляем список доступных игр
        loadAvailableGames();
    } catch (error) {
        console.error('Ошибка при выходе из игры:', error);
        alert('Ошибка: ' + error.message);
    }
}

function generateUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Инициализация при загрузке страницы
document.addEventListener('DOMContentLoaded', function() {
    if (typeof updateNavigation === 'function') {
        updateNavigation();
    }
    
    if (!checkAuth()) {
        return;
    }
    
    // Кнопки выбора режима
    document.getElementById('computerModeBtn').addEventListener('click', showComputerMode);
    document.getElementById('playerModeBtn').addEventListener('click', showPlayerMode);
    
    // Кнопки возврата в меню
    document.getElementById('backToMenuBtn').addEventListener('click', showGameModeSelection);
    document.getElementById('backToMenuBtn2').addEventListener('click', showGameModeSelection);
    
    // Кнопки для игры с компьютером
    document.getElementById('startFirstBtn').addEventListener('click', function() {
        initializeGame(false);
    });

    document.getElementById('startSecondBtn').addEventListener('click', function() {
        initializeGame(true);
    });
    
    // Кнопки для игры с игроком
    document.getElementById('createGameBtn').addEventListener('click', createPlayerGame);
    document.getElementById('refreshGamesBtn').addEventListener('click', loadAvailableGames);
    
    // Обработчик для кнопки выхода из игры (может быть не видна при загрузке)
    const leaveGameBtn = document.getElementById('leaveGameBtn');
    if (leaveGameBtn) {
        leaveGameBtn.addEventListener('click', leavePlayerGame);
    }
    
    // Обработчики кликов по клеткам для игры с компьютером
    document.getElementById('gameBoard').querySelectorAll('.cell').forEach(cell => {
        cell.addEventListener('click', function() {
            if (gameMode !== 'computer' || gameFinished || !isPlayerTurn || !currentGameId) {
                return;
            }
            
            const row = parseInt(this.dataset.row);
            const col = parseInt(this.dataset.col);
            
            if (gameBoard[row][col] !== 0) {
                return;
            }
            
            gameBoard[row][col] = 1;
            updateBoard('gameBoard');
            
            isPlayerTurn = false;
            document.getElementById('gameStatus').textContent = 'Ожидание ответа сервера...';
            
            makeComputerMove();
        });
    });
    
    // Обработчики кликов по клеткам для игры с игроком
    document.getElementById('playerGameBoardElement').querySelectorAll('.cell').forEach(cell => {
        cell.addEventListener('click', function() {
            if (gameMode !== 'player' || !currentGameId) {
                return;
            }
            
            const userId = localStorage.getItem('userId');
            if (!userId) return;
            
            const isMyTurn = currentPlayerId && currentPlayerId === userId;
            
            if (!isMyTurn) {
                return;
            }
            
            const row = parseInt(this.dataset.row);
            const col = parseInt(this.dataset.col);
            
            if (gameBoard[row][col] !== 0) {
                return;
            }
            
            gameBoard[row][col] = currentPlayerSymbol;
            updatePlayerBoard(gameBoard);
            
            makePlayerMove();
        });
    });
    
    // Показываем меню выбора режима по умолчанию
    showGameModeSelection();
});
