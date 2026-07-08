// +----------------------------------------------------------------------------------------------+
// +-[   copyright ] Sigmatek GmbH & CoKG                                                         |
// +-[      author ] kolott                                                                       |
// +-[        date ] 04.09.2014                                                                   |
// +-[ description ]------------------------------------------------------------------------------+
// |                                                                                              |
// |                                                                                              |
// +----------------------------------------------------------------------------------------------+

#include "SigCLib.h" 

unsigned long sigclib_htonl(unsigned long hostlong)
{
//  return hostlong;
  unsigned long b3 = (hostlong >> 24) & 0xFF;
  unsigned long b2 = (hostlong >> 16) & 0xFF;
  unsigned long b1 = (hostlong >>  8) & 0xFF;
  unsigned long b0 = (hostlong      ) & 0xFF;
  return (b3 | (b2<<8) | (b1<<16) | (b0<<24));
}

unsigned short sigclib_htons(unsigned short hostshort)
{
//  return hostshort;
  unsigned short b1 = (hostshort >> 8) & 0xFF;
  unsigned short b0 = (hostshort     ) & 0xFF;
  return ((b1) | (b0<<8));
}

unsigned long sigclib_ntohl(unsigned long hostlong)
{
//  return hostlong;
  unsigned long b3 = (hostlong >> 24) & 0xFF;
  unsigned long b2 = (hostlong >> 16) & 0xFF;
  unsigned long b1 = (hostlong >>  8) & 0xFF;
  unsigned long b0 = (hostlong      ) & 0xFF;
  return (b3 | (b2<<8) | (b1<<16) | (b0<<24));
}

unsigned short sigclib_ntohs(unsigned short hostshort)
{
//  return hostshort;
  unsigned short b1 = (hostshort >> 8) & 0xFF;
  unsigned short b0 = (hostshort     ) & 0xFF;
  return ((b1) | (b0<<8));
}
