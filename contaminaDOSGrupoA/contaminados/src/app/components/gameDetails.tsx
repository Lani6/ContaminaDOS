import React, { useState, useEffect } from "react";
import CustomModal from "./customModal";

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

interface GameDetailsProps {
  selectedGame: Game;
  playerName: string;
  gamePassword: string;
  isOwner: boolean;
  view: string;
  setView: (view: string) => void;
  setSelectedGame: (game: Game) => void;
  backEndAddress: string;
}

const GameDetails: React.FC<GameDetailsProps> = ({
  selectedGame,
  playerName,
  gamePassword,
  isOwner,
  view,
  setView,
  setSelectedGame,
  backEndAddress,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [modalTitle, setModalTitle] = useState("");
  const [modalMessage, setModalMessage] = useState("");

  const handleCloseModal = () => setShowModal(false);

  useEffect(() => {
    const intervalId = setInterval(() => {
      if (selectedGame) {
        handleRefreshGame();
      }
    }, 5000); // Poll every 5 seconds

    return () => clearInterval(intervalId); // Clean up on component unmount
  }, [selectedGame, view]);

  const handleRefreshGame = async () => {
    if (!playerName) {
      setModalTitle("Error");
      setModalMessage(
        "El nombre del jugador es requerido para refrescar la información del juego."
      );
      setShowModal(true);
      return;
    }

    // Configura los headers de manera condicional
    const headers: HeadersInit = {
      accept: "application/json",
      player: playerName,
    };

    if (gamePassword && gamePassword.trim()) {
      headers.password = gamePassword.trim();
    }

    try {
      const response = await fetch(
        `${backEndAddress}/api/games/${selectedGame.id}`,
        {
          method: "GET",
          headers: headers,
        }
      );
      if (response.ok) {
        const result: { data: Game } = await response.json();
        setSelectedGame(result.data);

        if (result.data.status === "rounds") {
          setView("gameStarted");
        }
      } else {
        setModalTitle("Error");
        setModalMessage("Error al refrescar el juego");
        setShowModal(true);
      }
    } catch (error) {
      setModalTitle("Error");
      setModalMessage("Error al refrescar el juego: " + error);
      setShowModal(true);
      throw new Error("Error al refrescar el juego:" + error);
    }
  };

  const handleStartGameErrors = (response: Response) => {
    if (response.status === 401) {
      setModalTitle("Error");
      setModalMessage("No autorizado para iniciar el juego.");
    } else if (response.status === 403) {
      setModalTitle("Error");
      setModalMessage("Acceso prohibido.");
    } else if (response.status === 404) {
      setModalTitle("Error");
      setModalMessage("Juego no encontrado.");
    } else if (response.status === 409) {
      setModalTitle("Error");
      setModalMessage("El juego ya ha sido iniciado.");
    } else if (response.status === 428) {
      setModalTitle("Error");
      setModalMessage(
        "Se necesitan al menos 5 jugadores para iniciar el juego."
      );
    } else {
      setModalTitle("Error");
      setModalMessage("Error desconocido al intentar iniciar el juego.");
    }
    setShowModal(true);
  };

  const handleStartGame = async () => {
    if (!selectedGame || !selectedGame.players) {
      setModalTitle("Error");
      setModalMessage("No hay información suficiente sobre los jugadores.");
      setShowModal(true);
      return;
    }

    const playerCount = selectedGame.players.length;

    if (playerCount < 5) {
      setModalTitle("Error");
      setModalMessage(
        "Se necesitan al menos 5 jugadores para iniciar el juego."
      );
      setShowModal(true);
      return;
    }

    if (playerCount > 10) {
      setModalTitle("Error");
      setModalMessage("No puede haber más de 10 jugadores en el juego.");
      setShowModal(true);
      return;
    }

    // Configura los headers de manera condicional
    const headers: HeadersInit = {
      accept: "application/json",
      player: playerName,
    };

    if (gamePassword && gamePassword.trim()) {
      headers.password = gamePassword.trim();
    }

    try {
      const response = await fetch(
        `${backEndAddress}/api/games/${selectedGame.id}/start`,
        {
          method: "HEAD",
          headers: headers,
        }
      );

      if (response.ok) {
        setModalTitle("Éxito");
        setModalMessage("Juego iniciado correctamente");
        setShowModal(true);

        handleRefreshGame();
        setView("gameStarted");
      } else {
        handleStartGameErrors(response);
      }
    } catch (error) {
      setModalTitle("Error");
      setModalMessage("Error al iniciar el juego: " + error);
      setShowModal(true);
      throw new Error("Error en la petición:" + error);
    }
  };

  return (
    <div className="container-game">
      <h2>Detalles de la Partida: {selectedGame.name}</h2>
      <p>Propietario: {selectedGame.owner}</p>
      <p>Estado: {selectedGame.status}</p>
      <p>Contraseña: {selectedGame.password ? "Sí" : "No"}</p>
      <p>Ronda Actual: {selectedGame.currentRound}</p>
      <h3>Jugadores:</h3>
      {selectedGame.players && selectedGame.players.length > 0 ? (
        <ul>
          {selectedGame.players.map((player, index) => (
            <li key={index} className="player-card">{player}</li>
          ))}
        </ul>
      ) : (
        <p>No hay jugadores en la partida.</p>
      )}
      <div className="button-group">
        <button
          type="button"
          className="btn btn-secondary"
          onClick={() => setView("list")}
        >
          Volver a la Lista
        </button>
        <button
          type="button"
          className="btn btn-info"
          onClick={handleRefreshGame}
        >
          Refrescar Información
        </button>
        {isOwner && (
          <button
            type="button"
            className="btn btn-primary"
            onClick={handleStartGame}
          >
            Iniciar Juego
          </button>
        )}
      </div>
      <CustomModal
        show={showModal}
        handleClose={handleCloseModal}
        title={modalTitle}
        message={modalMessage}
      />
    </div>
  );
};

export default GameDetails;
