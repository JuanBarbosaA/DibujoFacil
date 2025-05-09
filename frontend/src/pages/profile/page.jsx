import React, { useEffect, useState } from 'react';
import Logout from '../../Components/logout';
import { Link, useNavigate } from 'react-router-dom';

export default function ProfilePage() {
  const [user, setUser] = useState(null);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const fetchProfile = async () => {
      const token = localStorage.getItem('token');
      try {
        const res = await fetch('http://localhost:5054/api/User/profile', {
          method: 'GET',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        });

        if (!res.ok) throw new Error('Error al cargar el perfil');
        const data = await res.json();
        setUser(data);
      } catch (err) {
        setError(err.message);
      }
    };

    fetchProfile();
  }, []);

  if (error) return <p className="text-red-500 text-center p-4">{error}</p>;
  if (!user) return <p className="text-center p-4">Cargando...</p>;

  return (
    <div className="p-6 max-w-4xl mx-auto min-h-screen">
      <div className="mb-8 border-b pb-6 relative">
        <div className="flex justify-between items-start">
          <div className="flex items-center gap-4">
            {user.avatarUrl && (
              <img
                src={user.avatarUrl}
                alt="Avatar"
                className="w-16 h-16 rounded-full object-cover border border-gray-300"
              />
            )}
            <div>
              <h2 className="text-3xl font-bold text-gray-800 mb-2">
                Bienvenido, {user.name || user.email}
              </h2>
              <p className="text-gray-600 mb-1">Email: {user.email}</p>
              <p className="text-gray-600">
                Puntos:
                <span className="ml-2 bg-purple-100 text-purple-800 px-3 py-1 rounded-full text-sm">
                  {user.points}
                </span>
              </p>
            </div>
          </div>
          <Logout className="absolute top-0 right-0" />
        </div>
  
        <Link
          to="/createTutorial"
          className="mt-4 inline-block bg-green-500 text-white px-6 py-2 rounded-lg hover:bg-green-600 transition-colors"
        >
          Crear Tutorial
        </Link>
      </div>
  
      <div>
        <h3 className="text-2xl font-semibold text-gray-800 mb-6">
          Tus Tutoriales ({user.tutorials?.length || 0})
        </h3>
  
        {user.tutorials?.length > 0 ? (
          <div className="grid gap-6 md:grid-cols-2">
            {user.tutorials.map(tutorial => (
              <div
                key={tutorial.id}
                className="bg-white p-6 rounded-xl shadow-sm border border-gray-100 hover:shadow-md transition-shadow relative"
              >
                <div className="absolute top-4 right-4 flex gap-2">
                  <button
                    onClick={() => navigate(`/edit-tutorial/${tutorial.id}`)}
                    className="bg-blue-600 text-white px-3 py-1 rounded-md text-sm hover:bg-blue-700 transition-colors"
                  >
                    Editar
                  </button>
                </div>
  
                {tutorial.lastImage && (
                  <div className="mb-4 text-center">
                    <img
                      src={`data:${tutorial.lastImage.type};base64,${tutorial.lastImage.contentBase64}`}
                      alt="Miniatura del tutorial"
                      className="max-w-full max-h-48 mx-auto object-contain rounded-lg border border-gray-200"
                    />
                  </div>
                )}
  
                <h4 className="text-xl font-semibold text-gray-800 mb-2">{tutorial.title}</h4>
                <p className="text-gray-600 mb-4 line-clamp-3">{tutorial.description}</p>
  
                <div className="flex gap-4 items-center text-sm text-gray-500 mb-4">
                  <div className="flex items-center">
                    <span className="mr-1">📝</span>
                    {tutorial.commentCount}
                  </div>
  
                  <div className="flex items-center">
                    <span className="mr-1">⭐</span>
                    {tutorial.averageRating?.toFixed(1) || '0.0'}
                    <span className="ml-1 text-xs">
                      ({tutorial.ratingCount} votos)
                    </span>
                  </div>
                </div>
  
                <div className="flex justify-between items-center text-sm">
                  <span className="text-gray-500">
                    {new Date(tutorial.publicationDate).toLocaleDateString('es-ES', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </span>
  
                  <span
                    className={`px-2 py-1 rounded-md text-sm ${
                      tutorial.status === 'pending'
                        ? 'bg-amber-100 text-amber-800'
                        : 'bg-green-100 text-green-800'
                    }`}
                  >
                    {tutorial.status === 'pending' ? 'En revisión' : 'Publicado'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center p-8 border-2 border-dashed rounded-xl">
            <p className="text-gray-500 italic">
              Aún no has creado ningún tutorial
            </p>
            <Link
              to="/createTutorial"
              className="mt-4 inline-block text-blue-600 hover:underline"
            >
              Crear mi primer tutorial →
            </Link>
          </div>
        )}
      </div>
    </div>
  );
  
}