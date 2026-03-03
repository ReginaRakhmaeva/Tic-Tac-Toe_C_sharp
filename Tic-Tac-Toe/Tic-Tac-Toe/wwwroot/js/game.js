let currentGameId = null;
let gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
let isPlayerTurn = true;
let gameFinished = false;
let gameMode = null;
let currentPlayerSymbol = 1;
let currentPlayerId = null;
let player1Id = null;
let player2Id = null;
let refreshInterval = null;

function getAuthHeaders() {
    const headers = {
        'Content-Type': 'application/json'
    };
    
    const accessToken = localStorage.getItem('accessToken');
    if (accessToken) {
        headers['Authorization'] = 'Bearer ' + accessToken;
    }
    
    return headers;
}

function checkAuth() {
    const accessToken = localStorage.getItem('accessToken');
    if (!accessToken) {
        document.getElementById('authWarning').style.display = 'block';
        return false;
    }
    document.getElementById('authWarning').style.display = 'none';
    return true;
}

function showGameModeSelection() {
    document.getElementById('gameModeSelection').style.display = 'block';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'none';
    document.getElementById('historyContainer').style.display = 'none';
    document.getElementById('leaderboardContainer').style.display = 'none';
    gameMode = null;
    currentGameId = null;
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
    
    if (typeof showNavigation === 'function') {
        showNavigation();
    }
}

function showComputerMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'block';
    document.getElementById('playerGameContainer').style.display = 'none';
    document.getElementById('historyContainer').style.display = 'none';
    document.getElementById('leaderboardContainer').style.display = 'none';
    gameMode = 'computer';
    currentGameId = null;
    gameBoard = [[0, 0, 0], [0, 0, 0], [0, 0, 0]];
    
    document.getElementById('gameBoard').style.display = 'none';
    document.getElementById('gameStatus').textContent = 'Выберите, кто начинает игру';
    document.getElementById('gameId').style.display = 'none';
    document.getElementById('errorMessage').style.display = 'none';
    
    isPlayerTurn = false;
    gameFinished = false;
    clearBoard('gameBoard');
}

function showPlayerMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'block';
    document.getElementById('historyContainer').style.display = 'none';
    document.getElementById('leaderboardContainer').style.display = 'none';
    document.getElementById('playerGameBoard').style.display = 'none';
    document.getElementById('availableGamesList').style.display = 'block'; // Показываем список
    
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

function showHistoryMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'none';
    document.getElementById('historyContainer').style.display = 'block';
    document.getElementById('leaderboardContainer').style.display = 'none';
    
    gameMode = 'history';
    currentGameId = null;
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }
    
    loadGameHistory();
}

function showLeaderboardMode() {
    document.getElementById('gameModeSelection').style.display = 'none';
    document.getElementById('computerGameContainer').style.display = 'none';
    document.getElementById('playerGameContainer').style.display = 'none';
    document.getElementById('historyContainer').style.display = 'none';
    document.getElementById('leaderboardContainer').style.display = 'block';

    gameMode = 'leaderboard';
    currentGameId = null;
    if (refreshInterval) {
        clearInterval(refreshInterval);
        refreshInterval = null;
    }

    loadLeaderboard();
}

async function loadAvailableGames() {
    try {
        document.getElementById('availableGamesList').style.display = 'block';
        document.getElementById('playerGameBoard').style.display = 'none';
        
        const response = await fetch('/game/available', {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                localStorage.removeItem('authCredentials');
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
            if (userId && String(userId) === String(player1Id)) {
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

            // Скрываем навигацию (шапку) уже при создании игры,
            // даже если второй игрок еще не присоединился
            if (typeof hideNavigation === 'function') {
                hideNavigation();
            }
            
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
        const userIdStr = String(userId);
        if (userIdStr === String(player1Id)) {
            currentPlayerSymbol = 1; // X
        } else if (userIdStr === String(player2Id)) {
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
    
    // Скрываем навигацию во время игры с игроком
    if (typeof hideNavigation === 'function') {
        hideNavigation();
    }
    
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
    let userId = localStorage.getItem('userId');
    
    // Если userId нет, пытаемся извлечь из токена
    if (!userId) {
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken) {
            try {
                const parts = accessToken.split('.');
                if (parts.length === 3) {
                    const payloadJson = atob(parts[1]);
                    const payload = JSON.parse(payloadJson);
                    if (payload.uuid) {
                        userId = payload.uuid;
                        localStorage.setItem('userId', userId);
                        console.log('UserId восстановлен из токена:', userId);
                    }
                }
            } catch (e) {
                console.error('Ошибка при извлечении userId из токена:', e);
            }
        }
    }
    
    if (!userId) {
        console.warn('UserId не найден, не могу определить статус игры');
        return;
    }
    
    // Преобразуем в строки для сравнения (на случай разных форматов GUID)
    const currentPlayerIdStr = game.currentPlayerId ? String(game.currentPlayerId) : null;
    const userIdStr = String(userId);
    const isMyTurn = currentPlayerIdStr && currentPlayerIdStr === userIdStr;
    
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
        const winnerIdStr = game.winnerId ? String(game.winnerId) : null;
        if (winnerIdStr === userIdStr) {
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
    // Преобразуем в строки для сравнения
    const player1IdStr = player1Id ? String(player1Id) : null;
    const player2IdStr = player2Id ? String(player2Id) : null;
    if (player1IdStr === userIdStr) {
        playerInfo = 'Вы играете за X';
    } else if (player2IdStr === userIdStr) {
        playerInfo = 'Вы играете за O';
    }
    document.getElementById('playerInfo').textContent = playerInfo;
}

// Опрос состояния игры (для PvP)
async function pollPlayerGame() {
    if (!currentGameId) return;
    
    // Восстанавливаем userId из токена, если его нет
    let userId = localStorage.getItem('userId');
    if (!userId) {
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken) {
            try {
                const parts = accessToken.split('.');
                if (parts.length === 3) {
                    const payloadJson = atob(parts[1]);
                    const payload = JSON.parse(payloadJson);
                    if (payload.uuid) {
                        userId = payload.uuid;
                        localStorage.setItem('userId', userId);
                    }
                }
            } catch (e) {
                console.error('Ошибка при извлечении userId из токена:', e);
            }
        }
    }
    
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
                        // Показываем навигацию при выходе из игры
                        if (typeof showNavigation === 'function') {
                            showNavigation();
                        }
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
                // Показываем навигацию при выходе из игры
                if (typeof showNavigation === 'function') {
                    showNavigation();
                }
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
                // Показываем навигацию при завершении игры
                if (typeof showNavigation === 'function') {
                    showNavigation();
                }
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
    
    // Скрываем навигацию во время игры с компьютером
    if (typeof hideNavigation === 'function') {
        hideNavigation();
    }
    
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
        
        const userIdStr = String(userId);
        const currentPlayerIdStr = currentPlayerId ? String(currentPlayerId) : null;
        const isMyTurn = currentPlayerIdStr && currentPlayerIdStr === userIdStr;
        
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
                localStorage.removeItem('accessToken');
                localStorage.removeItem('refreshToken');
                localStorage.removeItem('userId');
                localStorage.removeItem('userLogin');
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
                // Показываем навигацию при выходе из игры
                if (typeof showNavigation === 'function') {
                    showNavigation();
                }
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
        
        // Показываем навигацию при выходе из игры
        if (typeof showNavigation === 'function') {
            showNavigation();
        }
        
        // Обновляем список доступных игр
        loadAvailableGames();
    } catch (error) {
        console.error('Ошибка при выходе из игры:', error);
        alert('Ошибка: ' + error.message);
    }
}

async function loadGameHistory() {
    const historyList = document.getElementById('historyList');
    const errorMessage = document.getElementById('historyErrorMessage');
    
    try {
        historyList.innerHTML = '<div class="text-center"><p class="text-muted">Загрузка истории игр...</p></div>';
        errorMessage.style.display = 'none';
        
        const response = await fetch('/game/history', {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                alert('Сессия истекла. Пожалуйста, войдите снова.');
                window.location.href = '/Login';
                return;
            }
            const errorData = await response.json();
            throw new Error(errorData.message || 'Ошибка при загрузке истории игр');
        }

        const games = await response.json();
        renderGameHistory(games);
    } catch (error) {
        console.error('Ошибка при загрузке истории:', error);
        errorMessage.textContent = 'Ошибка при загрузке истории игр: ' + error.message;
        errorMessage.style.display = 'block';
        historyList.innerHTML = '<div class="text-center"><p class="text-danger">Не удалось загрузить историю игр</p></div>';
    }
}

async function loadLeaderboard() {
    const tableBody = document.getElementById('leaderboardBody');
    const errorMessage = document.getElementById('leaderboardErrorMessage');
    const topNInput = document.getElementById('leaderboardTopN');

    let topN = 10;
    if (topNInput) {
        const value = parseInt(topNInput.value, 10);
        if (!isNaN(value) && value > 0 && value <= 100) {
            topN = value;
        }
    }

    try {
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted">
                    Загрузка таблицы лидеров...
                </td>
            </tr>`;
        errorMessage.style.display = 'none';

        const response = await fetch(`/game/leaderboard?topN=${encodeURIComponent(topN)}`, {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (!response.ok) {
            if (response.status === 401) {
                alert('Сессия истекла. Пожалуйста, войдите снова.');
                window.location.href = '/Login';
                return;
            }
            const errorData = await response.json().catch(() => null);
            const msg = errorData && errorData.message
                ? errorData.message
                : 'Ошибка при загрузке таблицы лидеров';
            throw new Error(msg);
        }

        const leaderboard = await response.json();
        renderLeaderboard(leaderboard);
    } catch (error) {
        console.error('Ошибка при загрузке таблицы лидеров:', error);
        errorMessage.textContent = 'Ошибка при загрузке таблицы лидеров: ' + error.message;
        errorMessage.style.display = 'block';
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-danger">
                    Не удалось загрузить таблицу лидеров
                </td>
            </tr>`;
    }
}

function renderLeaderboard(entries) {
    const tableBody = document.getElementById('leaderboardBody');

    if (!entries || entries.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center text-muted">
                    Пока нет данных для таблицы лидеров
                </td>
            </tr>`;
        return;
    }

    const rowsHtml = entries.map((entry, index) => {
        const login = entry.login || 'Без логина';
        const wins = entry.wins || 0;
        const losses = entry.losses || 0;
        const draws = entry.draws || 0;
        const ratio = typeof entry.winRatio === 'number'
            ? entry.winRatio.toFixed(2)
            : '0.00';

        return `
            <tr>
                <th scope="row">${index + 1}</th>
                <td>${login}</td>
                <td>${wins}</td>
                <td>${losses}</td>
                <td>${draws}</td>
                <td>${ratio}</td>
            </tr>`;
    }).join('');

    tableBody.innerHTML = rowsHtml;
}

function renderGameHistory(games) {
    const historyList = document.getElementById('historyList');
    
    if (!games || games.length === 0) {
        historyList.innerHTML = '<div class="text-center"><p class="text-muted">У вас пока нет завершенных игр</p></div>';
        return;
    }

    const userId = localStorage.getItem('userId');
    
    const gamesHtml = games.map(game => {
        const createdAt = new Date(game.createdAt);
        const dateStr = createdAt.toLocaleDateString('ru-RU', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
        
        let resultText = '';
        let resultClass = '';
        
        if (game.status === 'Draw') {
            resultText = 'Ничья';
            resultClass = 'text-warning';
        } else if (game.status === 'PlayerWins') {
            if (game.winnerId && String(game.winnerId) === String(userId)) {
                resultText = 'Победа';
                resultClass = 'text-success fw-bold';
            } else {
                resultText = 'Поражение';
                resultClass = 'text-danger';
            }
        } else {
            resultText = game.status;
            resultClass = 'text-secondary';
        }
        
        const gameTypeText = game.gameType === 1 ? 'С игроком' : 'С компьютером';
        
        // Определяем логин соперника
        let opponentLogin = null;
        if (game.gameType === 1) { // Игра с игроком
            if (game.player1Id && String(game.player1Id) === String(userId)) {
                opponentLogin = game.player2Login;
            } else if (game.player2Id && String(game.player2Id) === String(userId)) {
                opponentLogin = game.player1Login;
            }
        }
        
        return `
            <div class="card mb-3">
                <div class="card-body">
                    <h5 class="card-title">
                        <span class="${resultClass}">${resultText}</span>
                        <small class="text-muted ms-2">${gameTypeText}</small>
                    </h5>
                    <p class="card-text">
                        <small class="text-muted">Дата: ${dateStr}</small>
                    </p>
                    <p class="card-text">
                        <small>ID игры: ${game.id.substring(0, 8)}...</small>
                    </p>
                    ${opponentLogin ? `<p class="card-text">
                        <small>Соперник: ${opponentLogin}</small>
                    </p>` : ''}
                </div>
            </div>
        `;
    }).join('');

    historyList.innerHTML = gamesHtml;
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
    // Восстанавливаем userId из токена, если он есть
    if (typeof ensureUserIdFromToken === 'function') {
        ensureUserIdFromToken();
    } else {
        // Если функция не загружена, извлекаем userId напрямую
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken && !localStorage.getItem('userId')) {
            try {
                const parts = accessToken.split('.');
                if (parts.length === 3) {
                    const payloadJson = atob(parts[1]);
                    const payload = JSON.parse(payloadJson);
                    if (payload.uuid) {
                        localStorage.setItem('userId', payload.uuid);
                    }
                }
            } catch (e) {
                console.error('Ошибка при извлечении userId из токена:', e);
            }
        }
    }
    
    if (typeof updateNavigation === 'function') {
        updateNavigation();
    }
    
    if (!checkAuth()) {
        return;
    }
    
    // Кнопки выбора режима
    document.getElementById('computerModeBtn').addEventListener('click', showComputerMode);
    document.getElementById('playerModeBtn').addEventListener('click', showPlayerMode);
    document.getElementById('historyBtn').addEventListener('click', showHistoryMode);
    document.getElementById('leaderboardBtn').addEventListener('click', showLeaderboardMode);
    
    // Кнопки возврата в меню
    document.getElementById('backToMenuBtn').addEventListener('click', showGameModeSelection);
    document.getElementById('backToMenuBtn2').addEventListener('click', showGameModeSelection);
    document.getElementById('backToMenuBtn3').addEventListener('click', showGameModeSelection);
    document.getElementById('backToMenuBtn4').addEventListener('click', showGameModeSelection);
    
    // Кнопка обновления истории
    document.getElementById('refreshHistoryBtn').addEventListener('click', loadGameHistory);
    // Кнопка обновления таблицы лидеров
    document.getElementById('refreshLeaderboardBtn').addEventListener('click', loadLeaderboard);
    
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
            
            const userIdStr = String(userId);
            const currentPlayerIdStr = currentPlayerId ? String(currentPlayerId) : null;
            const isMyTurn = currentPlayerIdStr && currentPlayerIdStr === userIdStr;
            
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
