#Kontraktparametre
t_0 = 25.0
n = 100.0
R = 65.0
pi = 80.0
r = 0.03

#Tekniske parametre
timePoints = 1001
delta = (n-t_0)/(timePoints-1.0)
age = seq(from = t_0, by = delta, to = n)


a <- rep(0,timePoints)
for (i in ((timePoints-1):1)) {
  a[i] <- a[i+1] + (1.0 - a[i+1] * r) * delta
}

W <- rep(0,timePoints)
for (i in (2:timePoints)) {
  W[i] <- W[i-1] + (pi + W[i-1] * r) * delta
}

b <- W/a

alpha <- seq(from = 0.5, to = 2, length.out = timePoints)

z <- matrix(nrow = timePoints,ncol = timePoints)
for (i in 1:timePoints) {
  for (j in 1:timePoints) {
    z[i,j] <- b[i]*alpha[j]
  }
}

z <- z[-1:-470,]
z <- z[-130:-531,]

contour(z, x = seq(60, 70, length.out = nrow(z)), y = seq(0.5, 2, length.out = ncol(z)))
