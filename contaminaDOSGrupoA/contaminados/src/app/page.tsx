"use client";
import React, { useState, useEffect, useRef } from "react";
import "bootstrap/dist/css/bootstrap.min.css";
import { FaCog } from "react-icons/fa";
import CreateGameform from "./components/createGameform";
import GameList from "./components/gameList";
import JoinGame, { joinGame } from "./components/joinGame";
import GameDetails from "./components/gameDetails";
import GameStart from "./components/gameStart"; // Import the GameStart component
import "bootstrap/dist/css/bootstrap.min.css";
import CustomModal from "./components/customModal";

interface Game {
  name: string;
  owner: string;
  password?: string;
  id?: string;
  players?: string[];
  status?: string;
  enemies?: string[];
  currentRound?: string;
}

interface JoinGameResult {
  success: boolean;
  data?: Game;
  error?: string;
}

export default function Home() {
  useEffect(() => {
    if (typeof window !== "undefined") {
      require("bootstrap/dist/js/bootstrap.bundle.min.js");
    }
  }, []);

  const [view, setView] = useState("home");
  const [selectedGame, setSelectedGame] = useState<Game | null>(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [gamePassword, setGamePassword] = useState("");
  const [playerName, setPlayerName] = useState(""); // Nombre del jugador
  const [isOwner, setIsOwner] = useState(false); // Indica si el jugador es el propietario
  const [backendAddress, setBackendAddress] = useState("");
  const [showBackendErrorModal, setShowBackendErrorModal] =
    useState<boolean>(false); // Modal para backendAddress vacío
  const [showSettings, setShowSettings] = useState(false);
  const [showErrorModal, setShowErrorModal] = useState<boolean>(false);
  const playerNameRef = useRef<HTMLInputElement>(null);
  const [citizensScore, setCitizensScore] = useState<number>(0); // Track citizens score
  const [enemiesScore, setEnemiesScore] = useState<number>(0); // Track enemies score

  const handleGameCreated = (game: Game, password: string) => {
    if (!validateBackendAddress()) return; // Validate backend address
    setSelectedGame(game);
    setGamePassword(password);
    setPlayerName(game.owner); // El propietario es el creador de la partida
    setIsOwner(true); // El creador es el propietario
    setView("gameDetails");
  };

  const handleJoinGame = async (
    gameId: string,
    playerName: string,
    password: string
  ) => {
    if (!validateBackendAddress()) return; // Validate backend address
    const result: JoinGameResult = await joinGame(
      gameId,
      playerName,
      password,
      backendAddress
    );
    if (result.success) {
      setSelectedGame(result.data || null);
      setGamePassword(password);
      setPlayerName(playerName); // Establece el nombre del jugador
      setIsOwner(false); // El jugador que se une no es el propietario
      setView("gameDetails");
    } else {
      setErrorMessage(result.error || "Error al unirse a la partida");
      setShowErrorModal(true);
    }
  };

  const validateBackendAddress = () => {
    if (backendAddress.trim() === "" ||
      backendAddress === null
    ) {
      setShowBackendErrorModal(true); // Mostrar modal si backendAddress es vacío o nulo
      return false;
    }
    return true;
  };

  const handleCloseErrorModal = () => {
    setShowErrorModal(false);
    if (playerNameRef.current) {
      playerNameRef.current.focus();
    }
  };

  const handleCloseBackendErrorModal = () => {
    setShowBackendErrorModal(false);
  };

  const handleSelectGame = (game: Game) => {
    if (!validateBackendAddress()) return;
    setSelectedGame(game);
    setView("joinGame");
  };

  const handleReload = () => {
    window.location.reload();
  };

  const handleSaveBackendAddress = () => {
    setShowSettings(false);
  };
  return (
    <div className="container-game">
      {view === "home" && (
        <>
          <button
            type="button"
            className="btn btn-secondary float-end"
            onClick={() => setShowSettings(true)}
          >
            <FaCog /> Configuración
          </button>
          <h1 className="mb-4">Bienvenido</h1>
          <div className="d-flex justify-content-around">
            <button
              className="btn btn-primary btn-lg"
              onClick={() => {
                if (!validateBackendAddress()) return; // Validate backend address
                setView("create");
              }}
            >
              Crear Partida
            </button>
            <button
              className="btn btn-success btn-lg"
              onClick={() => {
                if (!validateBackendAddress()) return; // Validate backend address
                setView("list");
              }}
            >
              Unirse a Partida
            </button>
          </div>
        </>
      )}
      {view === "create" && (
        <CreateGameform
          onGameCreated={handleGameCreated}
          onCancel={() => setView("home")}
          setErrorMessage={setErrorMessage}
          backendAddress={backendAddress}
        />
      )}
      {view === "list" && (
        <GameList
          onSelectGame={handleSelectGame}
          onBack={() => setView("home")}
          backEndAddress={backendAddress}
        />
      )}
      {/* Modal */}
      <div
        className={`modal fade ${showSettings ? "show" : ""}`}
        style={{ display: showSettings ? "block" : "none" }}
        tabIndex={-1}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title text-dark">Configuración</h5>
              <button
                type="button"
                className="btn-close"
                onClick={() => setShowSettings(false)}
              ></button>
            </div>
            <div className="modal-body">
              <div className="form-group">
                <label className="text-dark">Dirección del Backend</label>
                <input
                  type="text"
                  className="form-control"
                  value={backendAddress}
                  onChange={(e) => setBackendAddress(e.target.value)}
                />
              </div>
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-primary"
                onClick={handleSaveBackendAddress}
              >
                Guardar
              </button>
              <button
                type="button"
                className="btn btn-secondary"
                onClick={() => setShowSettings(false)}
              >
                Cerrar
              </button>
            </div>
          </div>
        </div>
      </div>
      {view === "joinGame" && (
        <JoinGame
          selectedGame={selectedGame}
          onJoinGame={handleJoinGame}
          onCancel={() => setView("list")}
          playerNameRef={playerNameRef}
          backEndAddress={backendAddress}
        />
      )}

      {view === "gameDetails" && selectedGame && (
        <>
          <GameDetails
            selectedGame={selectedGame}
            playerName={playerName}
            gamePassword={gamePassword}
            isOwner={isOwner}
            view={view}
            setView={setView}
            setSelectedGame={setSelectedGame}
            backEndAddress={backendAddress}
          />
        </>
      )}

      {view === "gameStarted" && selectedGame && (
        <GameStart
          selectedGame={{ ...selectedGame, id: selectedGame.id || "" }}
          playerName={playerName}
          gamePassword={gamePassword}
          view={view}
          setView={setView}
          backEndAddress={backendAddress}
        />
      )}
      {/* Modal de error */}
      <div
        className={`modal fade ${showErrorModal ? "show" : ""}`}
        style={{ display: showErrorModal ? "block" : "none" }}
        tabIndex={-1}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title" id="errorModalLabel">
                Error
              </h5>
              <button
                type="button"
                className="btn-close"
                data-bs-dismiss="modal"
                aria-label="Close"
                onClick={handleCloseErrorModal}
              ></button>
            </div>
            <div className="modal-body">{errorMessage}</div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                data-bs-dismiss="modal"
                onClick={handleCloseErrorModal}
              >
                Cerrar
              </button>
            </div>
          </div>
        </div>
      </div>
      {/* Modal de advertencia de backendAddress */}
      <div
        className={`modal fade ${showBackendErrorModal ? "show" : ""}`}
        style={{ display: showBackendErrorModal ? "block" : "none" }}
        tabIndex={-1}
      >
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title text-dark" id="backendErrorModalLabel">
                Dirección del Backend Requerida
              </h5>
              <button
                type="button"
                className="btn-close"
                onClick={handleCloseBackendErrorModal}
              ></button>
            </div>
            <div className="modal-body text-dark">
              Debes configurar una dirección de backend antes de continuar.
            </div>
            <div className="modal-footer">
              <button
                type="button"
                className="btn btn-secondary"
                onClick={handleCloseBackendErrorModal}
              >
                Cerrar
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
