package com.example.clonedb.service;

import com.example.clonedb.repository.CompanyRepository;
import org.springframework.stereotype.Service;

@Service
public class AuthService {
    private final CompanyRepository companyRepository;

    public AuthService(CompanyRepository companyRepository) {
        this.companyRepository = companyRepository;
    }

    public boolean authenticate(String username, String password) {
        return companyRepository.findByUsername(username)
                .map(company -> company.getPassword().equals(password))
                .orElse(false);
    }
}
