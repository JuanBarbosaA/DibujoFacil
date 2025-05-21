import React, { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import axios from 'axios';

export default function VerifyEmail(){
    const [params] = useSearchParams();
    const [message, setMessage] = useState('');
    const [error, setError] = useState('');
    const navigate = useNavigate();
    const token = params.get('token');

    useEffect(() => {
        const verify = async () => {
            try {
                await axios.post('http://localhost:5054/api/auth/verify-email', { token });
                setMessage('¡Correo verificado exitosamente!');
                setTimeout(() => navigate('/login'), 3000);
            } catch (err) {
                setError(err.response?.data?.message || 'Error al verificar');
            }
        };
        
        token && verify();
    }, [token, navigate]);

    return (
        <div className="verification-container">
            <h2>Verificación de Correo</h2>
            {message && <div className="success">{message}</div>}
            {error && <div className="error">{error}</div>}
        </div>
    );
};