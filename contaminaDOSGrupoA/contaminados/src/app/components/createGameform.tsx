"use client";
import React, { useState } from "react";
import Button from "react-bootstrap/Button";
import Modal from "react-bootstrap/Modal";

interface Game {
  name: string;
  owner: string;
  password?: string;
}

interface ApiResponse {
  status: number;
  msg: string;
  data: Game;
  others: any;
}

interface createGameFormProps {
  onGameCreated: (game: Game, password: string) => void;
  onCancel: () => void;
  setErrorMessage: (message: string) => void;
  backendAddress: string;
}

const createGameForm: React.FC<createGameFormProps> = ({
  onGameCreated,
  onCancel,
  setErrorMessage,
  backendAddress,
}) => {
  const [gameDetails, setGameDetails] = useState<Game>({
    name: "",
    owner: "",
    password: "",
  });

  const [showErrorModal, setShowErrorModal] = useState(false);
  const [errorMessage, setErrorMessageLocal] = useState("");

  const validateInputs = (game: Game) => {
    if (game.name.length < 3) {
      setErrorMessageLocal(
        "El nombre de la partida debe tener al menos 3 caracteres."
      );
      return false;
    }
    if (game.owner.length < 3) {
      setErrorMessageLocal(
        "El nombre del propietario debe tener al menos 3 caracteres."
      );
      return false;
    }
    if (game.owner.length > 20) {
      setErrorMessageLocal(
        "El nombre del propietario debe tener menos de 20 caracteres."
      );
      return false;
    }
    if (game.password && (game.password.length > 0 && game.password.length < 3)) {
      setErrorMessageLocal("La contraseña debe tener al menos 3 caracteres.");
      return false;
    }
    if (game.password && game.password.length > 20) {
      setErrorMessageLocal("La contraseña debe tener menos de 20 caracteres.");
      return false;
    }

    return true;
  };

  const createGame = async (game: Game) => {
    if (!validateInputs(game)) {
      setShowErrorModal(true);
      return;
    }

    try {
      // Aquí aseguramos que si no hay contraseña, se envíe como ""
      const gameData: any = {
        name: game.name,
        owner: game.owner,
      };

      if (game.password?.trim()) {
        gameData.password = game.password.trim();
      }
      const response = await fetch(
        `${backendAddress}/api/games`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
          },
          body: JSON.stringify(gameData),
        }
      );

      if (response.ok) {
        const result: ApiResponse = await response.json();
        onGameCreated(result.data, game.password || "");
      } else if (response.status === 409) {
        setErrorMessageLocal("Ya existe una partida con ese nombre.");
        setShowErrorModal(true);
      } else {
        setErrorMessage("Error al crear la partida.");
        setShowErrorModal(true);
        throw new Error("Error al crear la partida");
      }
    } catch (error) {
      setErrorMessage("Error en la petición: " + error);
      throw new Error("Error en la petición:" + error);
    }
  };

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    createGame(gameDetails);
  };

  return (
    <form onSubmit={handleSubmit} className="container-game">
      <h2>Crear Partida</h2>
      <div className="mb-3">
        <label className="form-label">Nombre de la Partida</label>
        <input
          type="text"
          className="form-control"
          value={gameDetails.name}
          onChange={(e) =>
            setGameDetails({ ...gameDetails, name: e.target.value })
          }
          required
        />
      </div>
      <div className="mb-3">
        <label className="form-label">Propietario</label>
        <input
          type="text"
          className="form-control"
          value={gameDetails.owner}
          onChange={(e) =>
            setGameDetails({ ...gameDetails, owner: e.target.value })
          }
          required
        />
      </div>
      <div className="mb-3">
        <label className="form-label">Contraseña</label>
        <input
          type="password"
          className="form-control"
          value={gameDetails.password}
          onChange={(e) =>
            setGameDetails({ ...gameDetails, password: e.target.value })
          }
        />
      </div>
      <button type="submit" className="btn btn-primary">
        Crear
      </button>
      <button
        type="button"
        className="btn btn-secondary ms-2"
        onClick={onCancel}
      >
        Cancelar
      </button>

      <Modal show={showErrorModal} onHide={() => setShowErrorModal(false)}>
        <Modal.Header closeButton>
          <Modal.Title>Error</Modal.Title>
        </Modal.Header>
        <Modal.Body>{errorMessage}</Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={() => setShowErrorModal(false)}>
            Cerrar
          </Button>
        </Modal.Footer>
      </Modal>
    </form>
  );
};

export default createGameForm;
