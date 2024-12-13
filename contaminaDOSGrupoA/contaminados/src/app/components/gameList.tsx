import React, { useState, useEffect } from "react";
import CustomModal from "./customModal";

interface Game {
  id?: string;
  name: string;
  owner: string;
  status: string;
}

interface GameListProps {
  onSelectGame: (game: Game) => void;
  onBack: () => void;
  backEndAddress: string;
}

const GameList: React.FC<GameListProps> = ({
  onSelectGame,
  onBack,
  backEndAddress,
}) => {
  const [showModal, setShowModal] = useState(false);
  const [modalTitle, setModalTitle] = useState("");
  const [modalMessage, setModalMessage] = useState("");
  const handleCloseModal = () => setShowModal(false);
  const [games, setGames] = useState<Game[]>([]);
  const [filteredGames, setFilteredGames] = useState<Game[]>([]);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("lobby");
  const [currentPage, setCurrentPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const limitPerPage = 15;

  useEffect(() => {
    // Función para obtener todas las partidas, recorriendo todas las páginas
    const fetchAllGames = async () => {
      setLoading(true);
      let allGames: Game[] = [];
      let page = 0;
      let fetchedData: Game[] = [];

      do {
        try {
          // Construcción del URL con los parámetros de búsqueda
          const url = new URL(`${backEndAddress}/api/games`);

          // Agregar el filtro de nombre si está definido
          if (searchQuery.length >= 3) {
            url.searchParams.append("name", searchQuery);
          }

          // Agregar el filtro de estado si no es "Todos los estados"
          if (statusFilter) {
            url.searchParams.append("status", statusFilter);
          }

          url.searchParams.append("page", page.toString());
          url.searchParams.append("limit", "50");

          // Hacer la solicitud al backend por cada página
          const response = await fetch(url.toString());
          const result = await response.json();

          fetchedData = result.data as Game[]; // Asumimos que la respuesta sigue la estructura { data: Game[] }
          allGames = [...allGames, ...fetchedData];
          page += 1; // Avanzar a la siguiente página
        } catch (error) {
          setLoading(false);
          setModalTitle("Error");
          setModalMessage("Error al refrescar el juego: " + error);
          setShowModal(true);
        }
      } while (fetchedData.length > 0); // Detener si ya no hay más resultados

      setGames(allGames);
      setLoading(false);
    };

    fetchAllGames();
  }, [backEndAddress, searchQuery, statusFilter]);

  // Filtrar los juegos por nombre cada vez que se cambia la búsqueda o el estado
  useEffect(() => {
    if (searchQuery.length >= 3) {
      const filtered = games.filter((game) =>
        game.name.toLowerCase().includes(searchQuery.toLowerCase())
      );
      setFilteredGames(filtered);
    } else {
      setFilteredGames(games); // Si no hay búsqueda, mostrar todos los juegos
    }
  }, [searchQuery, games]);

  // Obtener las partidas a mostrar en la página actual
  const paginatedGames = filteredGames.slice(
    (currentPage - 1) * limitPerPage,
    currentPage * limitPerPage
  );

  const totalPages = Math.ceil(filteredGames.length / limitPerPage);

  return (
    <div className="game-list-container">
      <button className="back-button" onClick={onBack}>
        Regresar
      </button>

      <input
        type="text"
        className="search-input"
        placeholder="Buscar partida por nombre..."
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
      />

      <select
        value={statusFilter}
        onChange={(e) => setStatusFilter(e.target.value)}
      >
        <option value="">Todos los estados</option>{" "}
        {/* Nueva opción para buscar en todos los estados */}
        <option value="lobby">Lobby</option>
        <option value="rounds">Rounds</option>
        <option value="ended">Ended</option>
      </select>

      {loading ? (
        <p>Cargando partidas...</p>
      ) : (
        <div>
          <table className="game-table">
            <thead>
              <tr>
                <th>Nombre de la partida</th>
                <th>Propietario</th>
                <th>Estado</th>
                <th>Acción</th>
              </tr>
            </thead>
            <tbody>
              {paginatedGames.map((game) => (
                <tr key={game.id}>
                  <td>{game.name}</td>
                  <td>{game.owner}</td>
                  <td>{game.status}</td>
                  <td>
                    <button
                      className="select-button"
                      onClick={() => onSelectGame(game)}
                    >
                      Seleccionar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination-controls">
            <button
              className="pagination-button"
              onClick={() => setCurrentPage((prev) => Math.max(prev - 1, 1))}
              disabled={currentPage === 1}
            >
              Anterior
            </button>

            <span>
              Página {currentPage} de {totalPages}
            </span>

            <button
              className="pagination-button"
              onClick={() =>
                setCurrentPage((prev) => Math.min(prev + 1, totalPages))
              }
              disabled={currentPage === totalPages}
            >
              Siguiente
            </button>
          </div>
        </div>
      )}
      <CustomModal
        show={showModal}
        handleClose={handleCloseModal}
        title={modalTitle}
        message={modalMessage}
      />
    </div>
  );
};

export default GameList;

/*
const fetchAllGames = async () => {
    if (!validateFilters()) return;

    setError('');
    let allGamesFetched = [];
    let currentPage = 0;
    const limit = 50;

    try {
      while (true) {
        const url = new URL(envBackend + '/api/games');
        url.searchParams.append('limit', limit);
        url.searchParams.append('page', currentPage);

        const response = await fetch(url, {
          method: 'GET',
          headers: { Accept: 'application/json' },
        });

        if (response.ok) {
          const data = await response.json();
          if (Array.isArray(data.data) && data.data.length > 0) {
            allGamesFetched = [...allGamesFetched, ...data.data];

            if (data.data.length < limit) {
              break;
            }

            currentPage++;
          } else {
            setError('No hay partidas disponibles.');
            break;
          }
        } else {
          const errorData = await response.json();
          setError('Error al obtener partidas: ' + errorData.msg);
          toast.error('Error al obtener partidas: ' + errorData.msg);
          break;
        }
      }

      if (allGamesFetched.length > 0) {
        setAllGames(allGamesFetched);
        setFilteredGames(allGamesFetched);
        paginateGames(allGamesFetched, 0);
      } else {
        setAllGames([]);
        setFilteredGames([]);
        setDisplayedGames([]);
      }
    } catch (error) {
      setError('Error en la solicitud: ' + error.message);
      toast.error('Error en la solicitud: ' + error.message);
      setAllGames([]);
      setFilteredGames([]);
      setDisplayedGames([]);
    }
  };*/
