
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using WebApi.Services; 

[TestClass]
public class SieveServiceTests
{
    [TestMethod]
    public void GetNPrimes_10_ReturnsFirst10Primes()
    {    
        var sieveService = new SieveService(null); 
        int n = 10;
        List<int> expectedPrimes = new List<int> { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };
        List<int> actualPrimes = sieveService.GetNPrimes(n);
        CollectionAssert.AreEqual(expectedPrimes, actualPrimes);
    }
    [TestMethod]
    public void GetPrimesUpToN_10_ReturnsPrimesUpTo10()
    {        
        var sieveService = new SieveService(null);
        int n = 10;
        List<int> expectedPrimes = new List<int> { 2, 3, 5, 7 };        
        List<int> actualPrimes = sieveService.GetPrimesUpToN(n);  

        CollectionAssert.AreEqual(expectedPrimes, actualPrimes);
    }   
    [TestMethod]
    public void GetNPrimes_ZeroN_ReturnsEmptyList()
    {
    
        var sieveService = new SieveService(null);
        int n = 0;
        List<int> expectedPrimes = new List<int>();
        List<int> actualPrimes = sieveService.GetNPrimes(n);
        CollectionAssert.AreEqual(expectedPrimes, actualPrimes);
    }
       [TestMethod]
       [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void GetNPrimes_NegativeN_ThrowsArgumentOutOfRangeException()
        {           
            var sieveService = new SieveService(null);
            int n = -1;
           sieveService.GetNPrimes(n);
        }
}
