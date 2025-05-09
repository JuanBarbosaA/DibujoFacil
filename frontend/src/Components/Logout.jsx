import React from 'react';
import { useNavigate } from 'react-router-dom';

export default function Logout() {
  const navigate = useNavigate();

  const handleLogout = () => {
    localStorage.removeItem('token');
    navigate('/login'); 
  };

  return (
    <button
      onClick={handleLogout}
      className="text-blue-600 hover:underline font-semibold"
    >
      Cerrar Sesión
    </button>
  );
}
