import React, { useEffect, useState } from 'react';
import Logout from '../../Components/logout';
import { useNavigate } from 'react-router-dom';

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

  if (error) return <p style={{ color: 'red' }}>{error}</p>;
  if (!user) return <p>Cargando...</p>;

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto' }}>
      <div style={{ marginBottom: '30px' }}>
        <h2>Bienvenido, {user.name || user.email}</h2>
        <p>Email: {user.email}</p>
        <p>Puntos: {user.points}</p>
        <Logout />
      </div>

      <div>
        <h3>Tus Tutoriales ({user.tutorials?.length || 0})</h3>
        
        {user.tutorials?.length > 0 ? (
          <div style={{ display: 'grid', gap: '20px' }}>
            {user.tutorials.map(tutorial => (
              <div 
                key={tutorial.id}
                style={{
                  padding: '15px',
                  border: '1px solid #ddd',
                  borderRadius: '8px',
                  backgroundColor: '#f9f9f9',
                  position: "relative"
                }}
              >
                <div style={{ position: 'absolute', top: '10px', right: '10px' }}>
                <button 
                  onClick={() => navigate(`/edit-tutorial/${tutorial.id}`)}  // Usando navigate correctamente
                  style={{
                    padding: '5px 10px',
                    background: '#3b82f6',
                    color: 'white',
                    borderRadius: '5px',
                    border: 'none',
                    cursor: 'pointer'
                  }}
                >
                  Editar
                </button>
              </div>
                {tutorial.lastImage && (
                  <div style={{ marginBottom: '15px', textAlign: 'center' }}>
                    <img 
                      src={`data:${tutorial.lastImage.type};base64,${tutorial.lastImage.contentBase64}`}
                      alt="Miniatura del tutorial"
                      style={{ 
                        maxWidth: '100%',
                        height: '200px',
                        objectFit: 'contain',
                        borderRadius: '4px',
                        border: '1px solid #eee'
                      }}
                    />
                  </div>
                )}

                <h4>{tutorial.title}</h4>
                <p>{tutorial.description}</p>
                
                <div style={{ 
                  display: 'flex', 
                  gap: '15px', 
                  marginTop: '10px',
                  flexWrap: 'wrap'
                }}>
                  <div style={{ display: 'flex', gap: '5px', alignItems: 'center' }}>
                    <span>📝 {tutorial.commentCount}</span>
                  </div>
                  
                  <div style={{ display: 'flex', gap: '5px', alignItems: 'center' }}>
                    <span>⭐ {tutorial.averageRating?.toFixed(1) || '0.0'}</span>
                    <span style={{ fontSize: '0.9em', color: '#666' }}>
                      ({tutorial.ratingCount} votos)
                    </span>
                  </div>

                </div>
      

                <div style={{ 
                  display: 'flex', 
                  gap: '15px', 
                  marginTop: '10px',
                  justifyContent: 'space-between',
                  alignItems: 'center'
                }}>
                  <span style={{ fontSize: '0.9em', color: '#666' }}>
                    {new Date(tutorial.publicationDate).toLocaleDateString('es-ES', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit'
                    })}
                  </span>
                  
                  <span style={{
                    padding: '3px 8px',
                    borderRadius: '5px',
                    backgroundColor: tutorial.status === 'pending' ? '#fff3cd' : '#d4edda',
                    color: tutorial.status === 'pending' ? '#856404' : '#155724'
                  }}>
                    {tutorial.status === 'pending' ? 'En revisión' : 'Publicado'}
                  </span>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p style={{ color: '#666', fontStyle: 'italic' }}>
            Aún no has creado ningún tutorial
          </p>
        )}
      </div>
    </div>
  );
}